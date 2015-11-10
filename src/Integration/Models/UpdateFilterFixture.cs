using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Logs;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class UpdateFilterFixture : AdmIntegrationFixture
	{
		[Test]
		public void Load_updates_by_filter()
		{
			var user1 = DataMother.CreateTestClientWithUser().Users[0];
			var user2 = DataMother.CreateTestClientWithUser(session.Load<Region>(16ul)).Users[0];
			Flush();
			var update1 = new UpdateLogEntity(user1);
			Save(update1);
			var update2 = new UpdateLogEntity(user2);
			Save(update2);

			var filter = new UpdateFilter();
			filter.RegionMask = 16;
			filter.BeginDate = DateTime.Today.AddDays(-1);
			filter.EndDate = DateTime.Today;
			filter.UpdateType = UpdateType.Accumulative;
			var results = filter.Find(session);
			Assert.That(results.Count, Is.GreaterThan(0));
			Assert.That(results.Any(r => r.Id == update1.Id), Is.False, "нашли запись обновления в воронеже, {0}", update1.Id);
			Assert.That(results.Any(r => r.Id == update2.Id), Is.True, "не нашли запись обновления в челябинске, {0} {1}", update2.Id, results.Implode(r => r.Id));
			filter.UpdateType = UpdateType.AccessError;
			Save(new UpdateLogEntity(user2) {
				UpdateType = UpdateType.AutoOrder,
				RequestTime = DateTime.Now.AddHours(1),
				Commit = true
			});
			Save(new UpdateLogEntity(user2) {
				UpdateType = UpdateType.AccessError,
			});
			results = filter.Find(session);
			Assert.IsTrue(results.Any(r => r.OkUpdate));
		}

		[Test]
		public void Connect_client_app_logs()
		{
			var user = DataMother.CreateTestClientWithUser().Users[0];
			var clientLog = new ClientAppLog(user);
			clientLog.RequestToken = Guid.NewGuid().ToString();
			session.Save(clientLog);
			var requestLog = new RequestLog(user);
			requestLog.RequestToken = clientLog.RequestToken;
			session.Save(requestLog);

			var filter = new NewUpdateFilter {
				User = user
			};
			var logs = filter.Find(session);
			Assert.AreEqual(logs.Count, 1, logs.Implode());
			Assert.IsTrue(logs[0].HaveLog);
		}
	}
}