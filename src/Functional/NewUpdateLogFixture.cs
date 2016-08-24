using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers.Filters;
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
						CreatedOn = DateTime.Now.AddDays(-i),
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
			var filter = new NewUpdateFilter();

			filter.ErrorType = 0;
			TrySortDate(filter);
		}

		[Test]
		public void TrySortAnalitFNetClientAndUser()
		{
			var filter = new NewUpdateFilter();

			filter.Client = client;
			TrySortDate(filter);

			filter.User = client.Users.First();
			TrySortDate(filter);
		}

		private void TrySortDate(NewUpdateFilter filter, bool sum = false)
		{
			filter.BeginDate = DateTime.Now.Date.AddDays(-4);
			var logs = filter.Find(session);
			var count = logs.Count - 1;
			if (filter.SortDirection == "Desc") {
				for (int i = 0; i < count; i++) {
					Assert.That(logs[i].CreatedOn, Is.GreaterThanOrEqualTo(logs[i++].CreatedOn));
				}
			} else {
				for (int i = 0; i < count; i++) {
					Assert.That(logs[i].CreatedOn, Is.LessThanOrEqualTo(logs[i++].CreatedOn));
				}
			}
			if (!sum) {
				filter.SortDirection = filter.SortDirection == "Desc" ? "Asc" : "Desc";
				TrySortDate(filter, true);
			}
		}
	}
}