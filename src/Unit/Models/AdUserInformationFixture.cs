using System;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class AdUserInformationFixture
	{
		[Test]
		public void Update_last_logon_value_if_value_greater()
		{
			var info = new ADUserInformation();
			var value = DateTime.Now;
			info.CalculateLastLogon(value);
			Assert.That(info.LastLogOnDate, Is.EqualTo(value));

			info.CalculateLastLogon(null);
			Assert.That(info.LastLogOnDate, Is.EqualTo(value));

			info.CalculateLastLogon(value.AddDays(1));
			Assert.That(info.LastLogOnDate, Is.EqualTo(value.AddDays(1)));
		}

		[Test]
		public void CalculateLastLogonIfLogsNotNull()
		{
			var dtOld = DateTime.Now.AddHours(-1);
			var logs = new AuthorizationLogEntity {
				AFTime = dtOld,
				CITime = dtOld,
				AOLTime = dtOld,
				IOLTime = dtOld
			};
			var info = new ADUserInformation { Logs = logs };
			var value = DateTime.Now;

			info.CalculateLastLogon(value);
			Assert.That(info.LastLogOnDate, Is.EqualTo(value));

			info.Logs.AFTime = DateTime.Now.AddHours(1);

			info.CalculateLastLogon(value);
			Assert.That(info.LastLogOnDate, Is.EqualTo(null));

			info.Logs.AFTime = null;
			info.Logs.AOLTime = null;
			info.Logs.IOLTime = null;
			info.LastLogOnDate = null;

			info.CalculateLastLogon(value);
			Assert.That(info.LastLogOnDate, Is.EqualTo(value));
		}
	}
}