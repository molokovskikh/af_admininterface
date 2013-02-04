using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework.Test;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NHibernate.Linq;
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
		private AdditionalSettings options;
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
			controller.DbSession = session;

			((StubRequest)Request).Uri = new Uri("https://stat.analit.net/adm/Register/Register");
			((StubRequest)Request).ApplicationPath = "/Adm";

			client = new Client {
				Status = ClientStatus.On,
				Type = ServiceType.Drugstore,
				Name = "test " + Generator.Random(1000).First(),
				FullName = "test " + Generator.Random(1000).First(),
			};

			payer = new Payer {
				Name = "test",
			};

			regionSettings = new[] {
				new RegionSettings { Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true }
			};
			options = new AdditionalSettings { PayerExists = true };
			clientContacts = new[] { new Contact { Type = ContactType.Email, ContactText = "11@33.ru" } };
			person = new[] { new Person() };
		}

		[Test]
		public void Append_to_payer_comment_comment_from_payment_options()
		{
			client = DataMother.CreateTestClientWithUser();
			payer = client.Payers.First();

			payer.Comment = "ata";
			payer.Update();

			Context.Session["ShortName"] = "Test";

			var paymentOptions = new PaymentOptions { WorkForFree = true };

			controller.Registered(payer, paymentOptions, false);

			Assert.That(Payer.Find(payer.Id).Comment, Is.EqualTo("ata\r\nКлиент обслуживается бесплатно"));
		}

		[Test]
		public void Register_equeal_client_in_one_region()
		{
			Prepare();
			controller.RegisterClient(client, 1, regionSettings, null, options, null,
				null, null, clientContacts, new Contact[0], person, "11@ff.ru", "");
			controller.RegisterClient(client, 1, regionSettings, null, options, null,
				null, null, clientContacts, new Contact[0], person, "11@ff.ru", "");
			Assert.That(controller.Flash["Message"].ToString(), Is.StringContaining("В данном регионе уже существует клиент с таким именем"));
			controller.Flash["Message"] = null;
			controller.RegisterClient(client, 2, regionSettings, null, options, null,
				null, null, clientContacts, new Contact[0], person, "11@ff.ru", "");
			Assert.That(controller.Flash["Message"], Is.Null);
		}

		[Test]
		public void Create_organization()
		{
			session.Save(DataMother.CreateSupplier());

			Request.Params.Add("user.Accounting.IsFree", "False");
			Request.Params.Add("address.Value", "address");

			Prepare();

			controller.RegisterClient(client, 4, regionSettings, null, options, null,
				null, null, clientContacts, new Contact[0], person, "11@ff.ru", "");

			var registredClient = RegistredClient();
			var registredPayer = registredClient.Payers.Single();
			var registredUser = registredClient.Users.First();

			Assert.That(registredPayer.JuridicalOrganizations.Count, Is.EqualTo(1));
			var org = registredPayer.JuridicalOrganizations.Single();
			Assert.That(org.Name, Is.EqualTo(registredPayer.Name));
			Assert.That(org.FullName, Is.EqualTo(registredPayer.JuridicalName));
			Assert.That(registredClient.Addresses[0].LegalEntity, Is.EqualTo(org));

			var intersectionCount = registredClient.GetIntersectionCount();
			var userPriceCount = registredUser.GetUserPriceCount();
			var user = registredClient.Users.First();
			Assert.That(user.Accounting, Is.Not.Null);
			Assert.That(intersectionCount, Is.GreaterThan(0));
			Assert.That(userPriceCount, Is.GreaterThan(0));

			Assert.That(registredClient.Settings.SmartOrderRules.ParseAlgorithm, Is.EqualTo("TestSource"));
			Assert.That(registredClient.Settings.EnableSmartOrder, Is.EqualTo(true));
		}

		[Test]
		public void Register_client_without_address()
		{
			Prepare();
			Request.Params.Add("user.Accounting.IsFree", "False");

			var permissions = new[] { session.Query<UserPermission>().First(p => p.Shortcut == "AF") };
			controller.RegisterClient(client, 8, regionSettings,
				permissions,
				options, null,
				null, null, clientContacts, new Contact[0], person, "11@ff.ru", "");
			var registredClient = RegistredClient();

			var user = registredClient.Users[0];
			Assert.That(user.WorkRegionMask, Is.EqualTo(1));
			Assert.That(user.OrderRegionMask, Is.EqualTo(1));
			Assert.That(registredClient.Addresses.Count, Is.EqualTo(0));
			Assert.That(user.AssignedPermissions, Is.Not.Empty);
			var notDrugstorePermissions = user.AssignedPermissions
				.Where(p => p.AvailableFor != UserPermissionAvailability.Drugstore)
				.ToArray();
			Assert.That(notDrugstorePermissions.ToArray(), Is.Empty);
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
			var newPayer = new Payer {
				Name = "Тестовый плательщик",
				JuridicalName = "Тестовый плательщик"
			};
			controller.Registered(newPayer, new PaymentOptions(), false);

			Assert.That(newPayer.Id, Is.Not.EqualTo(0));
			Assert.That(newPayer.JuridicalOrganizations.Count, Is.EqualTo(1));
			var org = newPayer.JuridicalOrganizations[0];
			Assert.That(org.Name, Is.EqualTo("Тестовый плательщик"));
			Assert.That(org.FullName, Is.EqualTo("Тестовый плательщик"));
			Assert.That(notifications.Count, Is.EqualTo(1));
			Assert.That(notifications[0].Subject, Is.EqualTo("Зарегистрирован плательщик"));
		}

		[Test]
		public void SearchAllPayersTest()
		{
			var rnd = new Random();

			var newPayer = new Payer {
				Name = "Тестовый плательщик" + rnd.Next()
			};
			controller.Registered(newPayer, new PaymentOptions(), false);
			controller.SearchPayers("тестовый");
			var allPayers = (IEnumerable<Payer>)ControllerContext.PropertyBag["payers"];
			Assert.That(allPayers.Count(p => p.Name == newPayer.Name && p.Id == newPayer.Id), Is.EqualTo(1));
		}

		[Test]
		public void Register_supplier()
		{
			Request.Params.Add("supplier.Name", "Тестовый поставщик");
			Request.Params.Add("supplier.FullName", "Тестовый поставщик");
			Request.Params.Add("user.Name", "тестовый пользователь");

			controller.RegisterSupplier(
				new Contact[0], 1,
				new[] { new RegionSettings { Id = 1, IsAvaliableForBrowse = true } },
				new AdditionalSettings(),
				null,
				null,
				new Contact[0],
				new Person[0],
				"",
				"");
			var supplier = RegistredSupplier();
			Assert.That(supplier.Id, Is.GreaterThan(0));
			var user = supplier.Users.First();
			Assert.That(user.AssignedPermissions.Count(p => p.Type == UserPermissionTypes.SupplierInterface), Is.GreaterThan(0));
			var price = session.Query<Price>().First(p => p.Supplier.Id == supplier.Id);
			foreach (var priceRegionalData in price.RegionalData) {
				Assert.That(priceRegionalData.Cost.Id, Is.EqualTo(price.Costs[0].Id));
			}
		}

		[Test]
		public void Register_hidden_client()
		{
			Prepare();
			Request.Params.Add("client.Settings.IsHiddenFromSupplier", "True");

			controller.RegisterClient(client, 4, regionSettings, null, options, null,
				null, null, clientContacts, new Contact[0], person, "11@ff.ru", "");
			var registredClient = RegistredClient();
			Assert.That(registredClient.Settings.InvisibleOnFirm, Is.EqualTo(DrugstoreType.Hidden));
		}

		private void Prepare()
		{
			Request.Params.Add("user.Name", "Тестовый пользователь");
			Request.Params.Add("client.Settings.IgnoreNewPriceForUser", "False");
		}

		private Client RegistredClient()
		{
			var registredClient = session.Query<Client>()
				.OrderByDescending(c => c.Id)
				.FirstOrDefault(c => c.Registration.RegistrationDate >= begin);

			if (registredClient == null)
				throw new Exception("не зарегистрировалли клиента");
			return registredClient;
		}

		private Supplier RegistredSupplier()
		{
			var registred = session.Query<Supplier>()
				.OrderByDescending(c => c.Id)
				.FirstOrDefault(c => c.Registration.RegistrationDate >= begin);
			if (registred == null)
				throw new Exception("не зарегистрировалли клиента");
			return registred;
		}
	}
}