using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class AddressesControllerFixture : ControllerFixture
	{
		private AddressesController controller;
		private Client client;
		private User user;
		private DateTime begin;

		[SetUp]
		public void Setup()
		{
			begin = DateTime.Now;
			client = DataMother.CreateTestClientWithUser();
			user = client.Users.First();
			controller = new AddressesController();
			PrepareController(controller, "Addresses", "Add");
			controller.DbSession = session;
		}

		[Test]
		public void Add()
		{
			Request.Params.Add("address.Value", "тестовый адрес доставки");
			Request.Params.Add("address.AvaliableForUsers[0].Id", user.Id.ToString());
			controller.Add(new Contact[0], client.Id, "тестовое сообщение для биллинга");

			var messages = session.Query<AuditRecord>().Where(m => m.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "Сообщение в биллинг: тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Bind_legal_entity()
		{
			var begin = DateTime.Now;
			var payer = client.Payers[0];
			var org = new LegalEntity("Тестовый", payer);
			payer.JuridicalOrganizations.Add(org);
			session.Flush();

			Request.Params.Add("address.Value", "тестовый адрес доставки");
			Request.Params.Add("address.AvaliableForUsers[0].Id", user.Id.ToString());
			Request.Params.Add("address.LegalEntity.Id", org.Id.ToString());
			controller.Add(new Contact[0], client.Id, "тестовое сообщение для биллинга");

			var addresses = Registred();
			Assert.That(addresses.LegalEntity.Id, Is.EqualTo(org.Id));
		}

		[Test]
		public void Bind_account()
		{
			Request.Params.Add("address.Value", "тестовый адрес доставки");
			Request.Params.Add("address.Accounting.IsFree", "True");
			Request.Params.Add("address.Accounting.FreePeriodEnd", "12.04.2012");
			controller.Add(new Contact[0], client.Id, "тестовое сообщение для биллинга");

			Assert.That(Response.WasRedirected, Is.True);
			Assert.That(Context.Flash["Message"].ToString(), Is.EqualTo("Адрес доставки создан"));
			var address = Registred();
			Assert.That(address.Accounting.IsFree, Is.True);
			Assert.That(address.Accounting.FreePeriodEnd, Is.EqualTo(new DateTime(2012, 04, 12)));
		}

		private Address Registred()
		{
			var addresses = session.Query<Address>().Where(a => a.Registration.RegistrationDate >= begin).ToList();
			var address = addresses.First(a => a.Id == addresses.Max(x => x.Id));
			return address;
		}

		[Test]
		public void Change_address_organization()
		{
			var address = client.AddAddress("тестовый адрес доставки");
			address.Payer.Name = "Фарм-братан";
			address.Payer.JuridicalOrganizations[0].Name = "ООО Фарм-братан";
			var payer = DataMother.CreatePayer();
			payer.Name = "Фарм-друган";
			payer.JuridicalOrganizations[0].Name = "ООО Фарм-друган";
			client.Payers.Add(payer);
			session.Save(payer);
			session.SaveOrUpdate(client);
			Flush();

			address.LegalEntity = payer.JuridicalOrganizations.First();
			controller.Update(address, new Contact[0], new Contact[0]);

			Assert.That(address.Payer, Is.EqualTo(payer));
			var message = notifications.First();
			Assert.That(message.Body, Is.StringContaining("плательщик Фарм-братан юр.лицо ООО Фарм-братан"));
			Assert.That(message.Body, Is.StringContaining("плательщик Фарм-друган юр.лицо ООО Фарм-друган"));
		}
	}
}