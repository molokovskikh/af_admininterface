using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;
using IntegrationFixture = Test.Support.IntegrationFixture;

namespace Integration.Models
{
	[TestFixture]
	public class AuditLogRecordFixture : IntegrationFixture
	{
		private MonorailMailer mailer;

		[SetUp]
		public void Setup()
		{
			ForTest.InitializeMailer();
			mailer = ForTest.TestMailer(m => { });
		}

		[Test]
		public void Get_audit_logs_from_payer_audit_logs()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			user.Accounting.Payment = 1000;
			session.SaveOrUpdate(user);
			Flush();

			var logs = AuditLogRecord.GetLogs(user.Payer);
			Assert.AreEqual(3, logs.Count);
			Assert.AreEqual("Изменено 'Платеж' было '800,00000' стало '1000'", logs[0].Message, logs.Implode());
		}

		[Test]
		public void Log_comment_diff()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();

			payer.Comment += "\r\nтестовое сообщение";
			payer.CheckCommentChangesAndLog(mailer);
			payer.Save();
			Flush();

			var logs = new MessageQuery(LogMessageType.Stat).Execute(client, session);
			var log = logs.First();
			Assert.That(log.Message, Is.StringContaining("Изменено 'Комментарий'"));
			Assert.That(log.Message, Is.StringContaining("ins style"));
			Assert.That(log.IsHtml, Is.True);
		}

		[Test]
		public void Log_payer_comment_changes_for_all_clients()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers.First();

			var client1 = DataMother.TestClient(c => {
				c.Payers.Clear();
				c.Payers.Add(payer);
			});
			payer.Clients.Add(client1);
			payer.Comment += "\r\nтестовое сообщение";
			payer.CheckCommentChangesAndLog(mailer);
			payer.Save();
			Flush();

			var logs = new MessageQuery(LogMessageType.Stat).Execute(client, session);
			var log = logs.FirstOrDefault(m => m.Message.Contains("Изменено 'Комментарий'"));
			Assert.That(log, Is.Not.Null, logs.Implode());

			logs = new MessageQuery(LogMessageType.Stat).Execute(client1, session);
			log = logs.FirstOrDefault(m => m.Message.Contains("Изменено 'Комментарий'"));
			Assert.That(log, Is.Not.Null, logs.Implode());
		}
	}
}