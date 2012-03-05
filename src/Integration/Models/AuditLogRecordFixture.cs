using System;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;
using log4net.Config;
using IntegrationFixture = Test.Support.IntegrationFixture;

namespace Integration.Models
{
	[TestFixture]
	public class AuditLogRecordFixture : IntegrationFixture
	{
		[Test]
		public void Get_audit_logs_from_payer_audit_logs()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			user.Accounting.Payment = 1000;
			user.Save();
			scope.Flush();

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
			payer.Save();
			scope.Flush();

			var logs = ClientInfoLogEntity.MessagesForClient(client);
			var log = logs.First();
			Assert.That(log.Message, Is.StringContaining("Изменено 'Комментарий'"));
			Assert.That(log.Message, Is.StringContaining("ins style"));
			Assert.That(log.IsHtml, Is.True);
		}
	}
}