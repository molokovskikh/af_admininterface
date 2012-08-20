using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Integration.ForTesting;
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

			var logs = AuditRecord.Queryable.Where(l => l.ObjectId == client.Id).ToList();
			Assert.That(logs.FirstOrDefault(l => l.Message == "$$$Клиент отключен" && l.Type == LogObjectType.Client), Is.Not.Null, logs.Implode());
		}

		[Test]
		public void Update_supplier_status()
		{
			controller.UpdateClientStatus(supplier.Id, false, null);
			Flush();

			session.Refresh(supplier);
			Assert.That(supplier.Disabled, Is.True);

			var message = notifications.First();
			Assert.That(message.Subject, Is.EqualTo("Приостановлена работа поставщика"), notifications.Implode(n => n.Subject));
			var logs = AuditRecord.Queryable.Where(l => l.ObjectId == supplier.Id).ToList();
			Assert.That(logs.FirstOrDefault(l => l.Message == "$$$Клиент отключен" && l.Type == LogObjectType.Supplier), Is.Not.Null, logs.Implode());
		}
	}
}