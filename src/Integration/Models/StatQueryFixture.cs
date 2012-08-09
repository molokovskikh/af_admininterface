using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Helpers;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class StatQueryFixture
	{
		[Test]
		public void Max_date()
		{
			var query = new StatQuery();
			var data = query.Load(DateTime.Today, DateTime.Today);
			Assert.That(data.ToKeyValuePairs().Count(), Is.GreaterThan(0));
		}

		[Test]
		public void Fix_broke_dates()
		{
			var value = MainController.TryToFixProkenDateTimeValue("2012-01-17 17:53:13");
			Assert.That(value, Is.EqualTo("17:53:13"));
		}
	}
}