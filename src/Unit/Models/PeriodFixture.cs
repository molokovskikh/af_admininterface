using System;
using AdminInterface.Models.Billing;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class PeriodFixture
	{
		[Test]
		public void Get_Period_end()
		{
			var date = new DateTime(2011, 12, 10);
			var period = date.ToPeriod();
			Assert.That(period.GetPeriodEnd(), Is.EqualTo(new DateTime(2011, 12, 31, 23, 59, 59)));
		}
	}
}