using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Logs;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using NHibernate;
using NUnit.Framework;

namespace Functional
{
	[TestFixture]
	public class NewUpdateLogFixture : AdmSeleniumFixture
	{
		[Test]
		public void  TrySortStatisticAnalitFNet()
		{
			var client = DataMother.CreateTestClientWithUser();
			IEnumerable<RequestLog> updateLog = new List<RequestLog>();

			for (int i = 0; i < 2; i++) {
				updateLog = updateLog.Concat(new[] { new RequestLog(client.Users.First())
					{
						CreatedOn = DateTime.Now.AddDays(-i*2),
						Version = "1.11",
						IsCompleted = true,
						UpdateType = "MainController",
						ErrorType = 0
					}
				});
			}

			session.SaveEach(updateLog);

			Open(String.Format(
				"Logs/NewUpdateLog?filter.BeginDate={0}&filter.EndDate={1}&filter.ErrorType=0",
				DateTime.Now.AddDays(-2),
				DateTime.Now
				));
			AssertText("История обновлений AnalitF.net");

			var dates = browser.FindElementsByCssSelector("td[class='NotCommitedUpdate']");
			Assert.That(updateLog.First().CreatedOn.ToString(), Is.EqualTo(dates.First().Text));
			Assert.That(dates.First().Text, Is.GreaterThanOrEqualTo(dates.ElementAt(1).Text));

			Click("Дата");
			dates = browser.FindElementsByCssSelector("td[class='NotCommitedUpdate']");
			Assert.That(dates.First().Text, Is.LessThanOrEqualTo(dates.ElementAt(1).Text));
		}
	}
}