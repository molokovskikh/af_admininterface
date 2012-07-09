using System;
using AdminInterface.Helpers;
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
	}
}