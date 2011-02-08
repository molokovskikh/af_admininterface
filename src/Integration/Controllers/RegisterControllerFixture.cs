using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Integration.ForTesting;
using NUnit.Framework;
using Common.Web.Ui.Models;

namespace Integration.Controllers
{
	[TestFixture]
	public class RegisterControllerFixture : BaseControllerTest
	{
		private RegisterController controller;
		private Client client;
		private Payer payer;
		private RegionSettings[] regionSettings;
		private AdditionalSettings addsettings;
		private Contact[] clientContacts;
		private Person[] person;

		[SetUp]
		public void Setup()
		{
			var workerRequest = new SimpleWorkerRequest("", "", "", "http://test", new StreamWriter(new MemoryStream()));
			var context = new HttpContext(workerRequest);
			HttpContext.Current = context;

			controller = new RegisterController();
			PrepareController(controller, "Registered");
			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";

			client = DataMother.TestClient();
			payer = client.Payers.First();

			regionSettings = new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}
			};
			addsettings = new AdditionalSettings {PayerExists = true};
			clientContacts = new[] {new Contact{Id = 1, Type = 0, ContactText = "11@33.ru"}};
			person = new[] {new Person()};
		}

		[Test, Ignore("Чинить. Нужно вместо null передавать объект JuridicalOrganisation")]
		public void Append_to_payer_comment_comment_from_payment_options()
		{
			payer.Comment = "ata";
			payer.Update();

			Context.Session["ShortName"] = "Test";

			var paymentOptions = new PaymentOptions { WorkForFree = true };
			controller.Registered(payer, null, paymentOptions, client.Id, false);

			Assert.That(Payer.Find(payer.Id).Comment, Is.EqualTo("ata\r\nКлиент обслуживается бесплатно"));
		}

		[Test]
		public void Create_LegalEntity()
		{
			controller.RegisterClient(client, 1, regionSettings, null, addsettings, "address", payer, 
				payer.Id, null, clientContacts, null, new Contact[0], person, "11@ff.ru", "");

			payer.Refresh();
			Assert.That(payer.JuridicalOrganizations.Count, Is.EqualTo(1));
			var org = payer.JuridicalOrganizations.Single();
			Assert.That(org.Name, Is.EqualTo(payer.ShortName));
			Assert.That(org.FullName, Is.EqualTo(payer.JuridicalName));
			Assert.That(client.Addresses[0].LegalEntity, Is.EqualTo(org));
		}

		[Test]
		public void Create_client_with_smart_order()
		{
			controller.RegisterClient(client, 1, regionSettings, null, addsettings, "address", payer,
				payer.Id, null, clientContacts, null, new Contact[0], person, "", "");

			client = Client.Find(client.Id);
			Assert.That(client.Settings.SmartOrderRules.AssortimentPriceCode, Is.EqualTo(4662));
			Assert.That(client.Settings.SmartOrderRules.ParseAlgorithm, Is.EqualTo("TestSource"));
			Assert.That(client.Settings.EnableSmartOrder, Is.EqualTo(true));
		}
	}
}
