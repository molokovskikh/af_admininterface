using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models.Audit;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class BillingControllerFixture : ControllerFixture
	{
		private BillingController controller;
		private Supplier supplier;
		private Client client;

		[SetUp]
		public void Setup()
		{
			supplier = DataMother.CreateSupplier();
			session.Save(supplier);
			client = DataMother.TestClient();
			session.SaveOrUpdate(client);
			controller = new BillingController();
			controller.DbSession = session;
			PrepareController(controller);
		}

		[Test]
		public void Update_client_status()
		{
			controller.UpdateClientStatus(client.Id, false, null);
			Flush();

			var logs = session.Query<AuditRecord>().Where(l => l.ObjectId == client.Id).ToList();
			Assert.That(logs.FirstOrDefault(l => l.Message.Contains("$$$Изменено 'Включен'") && l.Type == LogObjectType.Client), Is.Not.Null, logs.Implode());
		}

		[Test]
		public void Update_supplier_status()
		{
			controller.UpdateClientStatus(supplier.Id, false, null);
			Flush();

			session.Refresh(supplier);
			Assert.That(supplier.Disabled, Is.True);

			var message = Emails.First();
			Assert.That(message.Subject, Is.EqualTo("Приостановлена работа поставщика"), Emails.Implode(n => n.Subject));
			var logs = session.Query<AuditRecord>().Where(l => l.ObjectId == supplier.Id).ToList();
			Assert.That(logs.FirstOrDefault(l => l.Message.Contains("$$$Изменено 'Включен' было 'вкл'") && l.Type == LogObjectType.Supplier), Is.Not.Null, logs.Implode());
		}

		[Test]
		public void UpdateSupplierStatusWithComment()
		{
			controller.UpdateClientStatus(supplier.Id, false, "тестовое отключение поставщика");
			var message = Emails.Last();
			Assert.That(message.Subject, Is.EqualTo("Приостановлена работа поставщика"), Emails.Implode(n => n.Subject));
			Assert.That(message.Body, Is.StringContaining("Причина отключения: тестовое отключение поставщика"));
			controller.UpdateClientStatus(supplier.Id, true, null);
			message = Emails.Last();
			Assert.That(message.Subject, Is.EqualTo("Возобновлена работа поставщика"));
			Assert.That(message.Body, Is.StringContaining("Причина отключения: тестовое отключение поставщика"));
		}

		[Test]
		public void UpdateClientStatusWithComment()
		{
			controller.UpdateClientStatus(client.Id, false, "тестовое отключение клиента");
			var message = Emails.Last();
			Assert.That(message.Subject, Is.EqualTo("Приостановлена работа клиента"), Emails.Implode(n => n.Subject));
			Assert.That(message.Body, Is.StringContaining("Причина отключения: тестовое отключение клиента"));
			controller.UpdateClientStatus(client.Id, true, null);
			message = Emails.Last();
			Assert.That(message.Subject, Is.EqualTo("Возобновлена работа клиента"), Emails.Implode(n => n.Subject));
			Assert.That(message.Body, Is.StringContaining("Причина отключения: тестовое отключение клиента"));
		}
	}
}