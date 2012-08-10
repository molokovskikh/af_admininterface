using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class AddressesControllerFixture : ControllerFixture
	{
		private AddressesController controller;
		private Client client;
		private User user;

		[SetUp]
		public void Setup()
		{
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

			var messages = AuditRecord.Queryable.Where(m => m.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "Сообщение в биллинг: тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Bind_account()
		{
			var begin = DateTime.Now;
			Request.Params.Add("address.Value", "тестовый адрес доставки");
			Request.Params.Add("address.Accounting.IsFree", "True");
			Request.Params.Add("address.Accounting.FreePeriodEnd", "12.04.2012");
			controller.Add(new Contact[0], client.Id, "тестовое сообщение для биллинга");

			Assert.That(Response.WasRedirected, Is.True);
			Assert.That(Context.Flash["Message"].ToString(), Is.EqualTo("Адрес доставки создан"));
			var addresses = Address.Queryable.Where(a => a.Registration.RegistrationDate >= begin).ToList();
			var address = addresses.First(a => a.Id == addresses.Max(x => x.Id));
			Assert.That(address.Accounting.IsFree, Is.True);
			Assert.That(address.Accounting.FreePeriodEnd, Is.EqualTo(new DateTime(2012, 04, 12)));
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
			payer.Save();
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