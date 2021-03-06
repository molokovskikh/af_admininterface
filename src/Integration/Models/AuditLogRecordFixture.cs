﻿using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Queries;
using Common.Tools;
using Common.Web.Ui.Models.Audit;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class AuditLogRecordFixture : AdmIntegrationFixture
	{
		private MonorailMailer mailer;
		private Client _client;

		[SetUp]
		public void Setup()
		{
			ForTest.InitializeMailer();
			mailer = ForTest.TestMailer(m => { });
			_client = DataMother.CreateTestClientWithUser();
		}

		[Test]
		public void Get_audit_logs_from_payer_audit_logs()
		{
			var user = _client.Users[0];
			user.Accounting.Payment = 1000;
			session.SaveOrUpdate(user);
			Flush();

			var logs = AuditLogRecord.GetLogs(session, user.Payer, false);
			Assert.That(logs.Count, Is.GreaterThan(0), logs.Implode(l => l.Message));
			Assert.AreEqual("Изменено 'Платеж' было '800' стало '1000'", logs[0].Message, logs.Implode());
		}

		[Test]
		public void Log_comment_diff()
		{
			var payer = _client.Payers.First();

			payer.Comment += "\r\nтестовое сообщение";
			payer.CheckCommentChangesAndLog(session, mailer);
			session.Save(payer);
			Flush();

			var logs = new MessageQuery(LogMessageType.Stat).Execute(_client, session);
			var log = logs.First();
			Assert.That(log.Message, Is.StringContaining("Изменено 'Комментарий'"));
			Assert.That(log.Message, Is.StringContaining("ins style"));
			Assert.That(log.IsHtml, Is.True);
		}

		[Test]
		public void Log_payer_comment_changes_for_all_clients()
		{
			var payer = _client.Payers.First();

			var client1 = DataMother.TestClient(c => {
				c.Payers.Clear();
				c.Payers.Add(payer);
			});
			payer.Clients.Add(client1);
			payer.Comment += "\r\nтестовое сообщение";
			payer.CheckCommentChangesAndLog(session, mailer);
			session.Save(payer);
			Flush();

			var logs = new MessageQuery(LogMessageType.Stat).Execute(_client, session);
			var log = logs.FirstOrDefault(m => m.Message.Contains("Изменено 'Комментарий'"));
			Assert.That(log, Is.Not.Null, logs.Implode());

			logs = new MessageQuery(LogMessageType.Stat).Execute(client1, session);
			log = logs.FirstOrDefault(m => m.Message.Contains("Изменено 'Комментарий'"));
			Assert.That(log, Is.Not.Null, logs.Implode());
		}

		private void Create_payer_message()
		{
			var payer = _client.Payers.First();

			var logRecord = new PayerAuditRecord(payer, "$$$test_Payer_Audit_Record");
			session.Save(logRecord);
			Flush();
		}

		[Test]
		public void Log_payer_with_payer_audit_record()
		{
			Create_payer_message();
			var messageQuery = new MessageQuery();
			messageQuery.Types.Add(LogMessageType.Payer);
			var logs = messageQuery.Execute(_client, session);

			var log = logs.FirstOrDefault(m => m.Message.Contains("test_Payer_Audit_Record"));
			Assert.That(log, Is.Not.Null, logs.Implode());

			messageQuery.Types.Remove(LogMessageType.Payer);

			logs = messageQuery.Execute(_client, session);

			log = logs.FirstOrDefault(m => m.Message.Contains("test_Payer_Audit_Record"));
			Assert.That(log, Is.Null, logs.Implode());
		}

		[Test]
		public void Log_payer_with_payer_and_user()
		{
			Create_payer_message();
			var messageQuery = new MessageQuery();
			messageQuery.Types.Add(LogMessageType.Payer);
			var logs = messageQuery.Execute(_client.Users.First(), session);

			var log = logs.FirstOrDefault(m => m.Message.Contains("test_Payer_Audit_Record"));
			Assert.That(log, Is.Not.Null, logs.Implode());
		}

		[Test]
		public void LogUserNoBilling()
		{
			var record = new AuditRecord("testUserAuditRecord", _client.Users.First());
			session.Save(record);
			Flush();
			var messageQuery = new MessageQuery();
			var logs = messageQuery.Execute(_client.Users.First(), session);
			var log = logs.FirstOrDefault(m => m.Message.Contains("testUserAuditRecord"));
			Assert.That(log, Is.Not.Null, logs.Implode());
		}

		[Test]
		public void LogUserWithShowOnlyPayer()
		{
			var payer = _client.Payers.First();

			var logRecord = new PayerAuditRecord(payer, "$$$testPayerAuditRecordUser");
			logRecord.ObjectType = LogObjectType.User;
			logRecord.ObjectId = _client.Users.First().Id;
			logRecord.ShowOnlyPayer = true;
			session.Save(logRecord);
			Flush();
			var messageQuery = new MessageQuery();
			messageQuery.Types.Add(LogMessageType.Payer);
			var logs = messageQuery.Execute(_client.Users.First(), session);
			var log = logs.FirstOrDefault(m => m.Message.Contains("testPayerAuditRecordUser"));
			Assert.That(log, Is.Null, logs.Implode());
			logs = messageQuery.Execute(_client, session);
			log = logs.FirstOrDefault(m => m.Message.Contains("testPayerAuditRecordUser"));
			Assert.That(log, Is.Not.Null, logs.Implode());
		}
	}
}