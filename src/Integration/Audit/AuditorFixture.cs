using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models.Audit;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AuditorFixture : AdmIntegrationFixture
	{
		[Test(Description = "Проверяет корректную работу обновления логов")]
		public void UpdateLogTest()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var oldName = user.Name;
			user.Name += "1";
			session.SaveOrUpdate(user);

			Flush();
			var newClient = DataMother.TestClient();
			session.Clear();
			AuditRecord.UpdateLogs(newClient.Id, user);
			var logs = session.Query<AuditRecord>().Where(l => l.ObjectId == user.Id && l.Type == LogObjectType.User).ToList();
			Assert.That(logs.Implode(m => m.Message),
				Is.StringContaining(String.Format("$$$Изменено 'Комментарий' было '{0}' стало '{1}'", oldName, user.Name)));
			Assert.That(logs[0].Service.Id, Is.EqualTo(newClient.Id));
		}

		[Test(Description = "Проверяет корректную работу обновления логов при совпадении идентификаторов у разных сущностей")]
		public void UpdateLogWithOtherTypeTest()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var oldName = user.Name;
			user.Name += "1";
			session.SaveOrUpdate(user);

			Flush();
			var logs = session.Query<AuditRecord>().Where(l => l.ObjectId == user.Id && l.Type == LogObjectType.User).ToList();
			Assert.That(logs.Implode(x => x.Message),
				Is.StringContaining(String.Format("$$$Изменено 'Комментарий' было '{0}' стало '{1}'", oldName, user.Name)));
			Assert.That(logs[0].Service.Id, Is.EqualTo(client.Id));
			session.Clear();
			var newClient = DataMother.TestClient();
			var address = new Address() {
				Id = user.Id
			};
			AuditRecord.UpdateLogs(newClient.Id, address);
			logs = session.Query<AuditRecord>().Where(l => l.ObjectId == user.Id && l.Type == LogObjectType.User).ToList();
			Assert.That(logs.Implode(x => x.Message),
				Is.StringContaining(String.Format("$$$Изменено 'Комментарий' было '{0}' стало '{1}'", oldName, user.Name)));
			Assert.That(logs[0].Service.Id, Is.EqualTo(client.Id));
		}

		[Test]
		public void Audit_client_changes()
		{
			var client = DataMother.TestClient();
			var oldName = client.Name;
			client.Name += "1";
			session.SaveOrUpdate(client);

			Flush();
			Reopen();

			var logs = session.Query<AuditRecord>().Where(l => l.ObjectId == client.Id && l.Type == LogObjectType.Client).ToList();
			Assert.That(logs[0].Message,
				Is.EqualTo(String.Format("$$$Изменено 'Краткое наименование' было '{0}' стало '{1}'", oldName, client.Name)));
		}

		[Test]
		public void Log_lazy_property()
		{
			var supplier = DataMother.CreateSupplier();
			var client = DataMother.TestClient();
			Save(supplier);
			Save(client);
			Flush();
			Reopen();

			client = session.Load<Client>(client.Id);
			var price = session.Load<Price>(supplier.Prices[0].Id);
			Assert.That(NHibernateUtil.IsInitialized(price), Is.False);
			client.Settings.AssortimentPrice = price;
			Save(client);
			Flush();

			var messages = new MessageQuery().Execute(client, session);
			var message = messages.First();
			Assert.That(message.Message,
				Is.EqualTo("$$$Изменено 'Ассортиментный прайс для преобразования накладной в формат dbf' было '' стало 'Тестовый поставщик - Базовый'"));
		}

		[Test]
		public void Audit_formatter_changes()
		{
			var supplier = DataMother.CreateSupplier();
			var oldFormat = OrderHandler.Formaters().First();
			var rule = new OrderSendRules {
				Supplier = supplier,
				Formater = oldFormat,
				Sender = OrderHandler.Senders().First()
			};
			supplier.OrderRules.Add(rule);
			Save(supplier);
			Flush();

			Assert.That(rule.Id, Is.Not.EqualTo(0));
			var newFormat = OrderHandler.Formaters().Skip(1).First();
			rule.Formater = newFormat;
			session.Save(rule);

			Flush();
			Reopen();

			var logs = session.Query<AuditRecord>().Where(l => l.ObjectId == supplier.Id && l.Type == LogObjectType.Supplier).ToList();
			Assert.That(logs.Count, Is.GreaterThan(0), "нет ни одного сообщения");
			Assert.That(logs[0].Message,
				Is.EqualTo(String.Format("$$$Изменено 'Форматер' было '{0}' стало '{1}'", oldFormat.ClassName, newFormat.ClassName)));
		}
	}
}