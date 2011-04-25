using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Models;

namespace Integration.Controllers
{
	[TestFixture]
	public class RegisterControllerFixture : ControllerFixture
	{
		private RegisterController controller;
		private Client client;
		private Payer payer;
		private RegionSettings[] regionSettings;
		private AdditionalSettings addsettings;
		private Contact[] clientContacts;
		private Person[] person;

		private DateTime begin;

		[SetUp]
		public void Setup()
		{
			begin = DateTime.Now;

			var workerRequest = new SimpleWorkerRequest("", "", "", "http://test", new StreamWriter(new MemoryStream()));
			var context = new HttpContext(workerRequest);
			HttpContext.Current = context;

			controller = new RegisterController();
			PrepareController(controller, "Registered");
			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";

			client = new Client {
				Status = ClientStatus.On,
				Segment = Segment.Wholesale,
				Type = ServiceType.Drugstore,
				Name = "test",
				FullName = "test",
			};

			payer = new Payer {
				Name = "test",
			};

			regionSettings = new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}
			};
			addsettings = new AdditionalSettings {PayerExists = true};
			clientContacts = new[] {new Contact{Id = 1, Type = 0, ContactText = "11@33.ru"}};
			person = new[] {new Person()};
		}

		[Test]
		public void Append_to_payer_comment_comment_from_payment_options()
		{
			client = DataMother.TestClient();
			payer = client.Payers.First();

			payer.Comment = "ata";
			payer.Update();

			Context.Session["ShortName"] = "Test";

			var paymentOptions = new PaymentOptions { WorkForFree = true };

			using(new SessionScope())
				controller.Registered(payer, payer.JuridicalOrganizations.First(), paymentOptions, false);

			Assert.That(Payer.Find(payer.Id).Comment, Is.EqualTo("ata\r\nКлиент обслуживается бесплатно"));
		}

		[Test]
		public void Create_organization()
		{
			controller.RegisterClient(client, 1, regionSettings, null, addsettings, "address", null, 
				null, null, clientContacts, null, new Contact[0], person, "11@ff.ru", "");

			var registredClient = RegistredClient();
			var registredPayer = registredClient.Payers.Single();

			Assert.That(registredPayer.JuridicalOrganizations.Count, Is.EqualTo(1));
			var org = registredPayer.JuridicalOrganizations.Single();
			Assert.That(org.Name, Is.EqualTo(registredPayer.Name));
			Assert.That(org.FullName, Is.EqualTo(registredPayer.JuridicalName));
			Assert.That(registredClient.Addresses[0].LegalEntity, Is.EqualTo(org));

			var intersectionCount = ArHelper.WithSession(
				s => s.CreateSQLQuery("select count(*) from Future.Intersection where clientId = :clientId")
					.SetParameter("clientId", registredClient.Id)
					.UniqueResult<long>());

			var user = registredClient.Users.First();
			Assert.That(user.Accounting, Is.Not.Null);
			Assert.That(intersectionCount, Is.GreaterThan(0));
		}

		[Test]
		public void Create_client_with_smart_order()
		{
			controller.RegisterClient(client, 1, regionSettings, null, addsettings, "address",
				null, null, null, clientContacts, null, new Contact[0], person, "", "");

			client = RegistredClient();
			Assert.That(client.Settings.SmartOrderRules.AssortimentPriceCode, Is.EqualTo(4662));
			Assert.That(client.Settings.SmartOrderRules.ParseAlgorithm, Is.EqualTo("TestSource"));
			Assert.That(client.Settings.EnableSmartOrder, Is.EqualTo(true));
		}

		[Test]
		public void Register_payer()
		{
			controller.RegisterPayer(0, false);

			var payer = (Payer)ControllerContext.PropertyBag["Instance"];
			Assert.That(payer, Is.Not.Null);
		}

		[Test]
		public void Payer_registration()
		{
			var newPayer = new Payer();
			controller.Registered(newPayer, new LegalEntity{Name = "test", FullName = "test full"}, new PaymentOptions(), false);

			Assert.That(newPayer.Id, Is.Not.EqualTo(0));
			Assert.That(newPayer.JuridicalOrganizations.Count, Is.EqualTo(1));
			var org = newPayer.JuridicalOrganizations[0];
			Assert.That(org.Name, Is.EqualTo("test"));
			Assert.That(org.FullName, Is.EqualTo("test full"));
		}

		private Client RegistredClient()
		{
			var registredClient = Client.Queryable.FirstOrDefault(c => c.Registration.RegistrationDate >= begin);
			if (registredClient == null)
				throw new Exception("не зарегистрировалли клиента");
			return registredClient;
		}
	}
}
