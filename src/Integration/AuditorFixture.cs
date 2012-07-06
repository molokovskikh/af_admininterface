using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NHibernate;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AuditorFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Audit_client_changes()
		{
			var client = DataMother.TestClient();
			var oldName = client.Name;
			client.Name += "1";
			client.Save();

			Flush();
			Reopen();

			var logs = ClientInfoLogEntity.Queryable.Where(l => l.ObjectId == client.Id && l.Type == LogObjectType.Client).ToList();
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

			client = Client.Find(client.Id);
			var price = Price.Find(supplier.Prices[0].Id);
			Assert.That(NHibernateUtil.IsInitialized(price), Is.False);
			client.Settings.AssortimentPrice = price;
			Save(client);
			Flush();

			var messages = new MessageQuery().Execute(client, session);
			var message = messages.Last();
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
			rule.Save();

			Flush();
			Reopen();

			var logs = ClientInfoLogEntity.Queryable.Where(l => l.ObjectId == supplier.Id && l.Type == LogObjectType.Supplier).ToList();
			Assert.That(logs.Count, Is.GreaterThan(0), "нет ни одного сообщения");
			Assert.That(logs[0].Message,
				Is.EqualTo(String.Format("$$$Изменено 'Форматер' было '{0}' стало '{1}'", oldFormat.ClassName, newFormat.ClassName)));
		}
	}
}
