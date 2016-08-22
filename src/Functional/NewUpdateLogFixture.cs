using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Web.Ui.NHibernateExtentions;
using Functional.ForTesting;
using NUnit.Framework;

namespace Functional
{
	[TestFixture]
	public class NewUpdateLogFixture : AdmSeleniumFixture
	{
		private Client client;
		private IEnumerable<RequestLog> updateLog = new List<RequestLog>();

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			for (int i = 0; i < 4; i++) {
				updateLog = updateLog.Concat(new[] { new RequestLog(client.Users.First()) {
						CreatedOn = DateTime.Now.AddHours(-i*2),
						Version = "1.11",
						IsCompleted = true,
						UpdateType = "MainController",
						ErrorType = (i % 2 == 0) ?  0 : 1
					}
				});
			}

			session.SaveEach(updateLog);
			CommitAndContinue();
		}

		[Test]
		public void  TrySortStatisticAnalitFNet()
		{
			Open("Logs/NewUpdateLog?filter.ErrorType=0");
			AssertText("История обновлений AnalitF.net");

			TrySortDate();
		}

		[Test]
		public void TrySortAnalitFNetClient()
		{
			Open(client);
			Click("История обновлений Analitf-net");
			OpenedWindow(String.Format("История обновлений AnalitF.net клиента {0}", client.Name));

			TrySortDate();
		}

		private void TrySortDate()
		{
			var dates = browser.FindElementsByCssSelector("td[class='NotCommitedUpdate']");
			Assert.That(dates.First().Text, Is.GreaterThanOrEqualTo(dates.ElementAt(1).Text));

			Click("Дата");
			dates = browser.FindElementsByCssSelector("td[class='NotCommitedUpdate']");
			Assert.That(dates.First().Text, Is.LessThanOrEqualTo(dates.ElementAt(1).Text));
		}
	}
}