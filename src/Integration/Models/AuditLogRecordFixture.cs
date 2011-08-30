using System;
using AdminInterface.Models.Billing;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;
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
			Assert.AreEqual("Изменено 'Платеж' было '800' стало '1000'", logs[2].Message, logs.Implode(l => l.Message));
		}
	}
}