using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AuditorFixture : IntegrationFixture
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
			supplier.Save();
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
