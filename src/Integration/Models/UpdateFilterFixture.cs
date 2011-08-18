using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class UpdateFilterFixture : IntegrationFixture
	{
		[Test]
		public void Load_updates_by_filter()
		{
			var user1 = DataMother.CreateTestClientWithUser().Users[0];
			var user2 = DataMother.CreateTestClientWithUser(Region.Find(16ul)).Users[0];
			var update1 = new UpdateLogEntity(user1);
			update1.Save();
			var update2 = new UpdateLogEntity(user2);
			update2.Save();

			var filter = new UpdateFilter();
			filter.RegionMask = 16;
			filter.BeginDate = DateTime.Today.AddDays(-1);
			filter.EndDate = DateTime.Today;
			filter.UpdateType = UpdateType.Accumulative;
			var results = filter.Find();
			Assert.That(results.Count, Is.GreaterThan(0));
			Assert.That(results.Any(r => r.Id == update1.Id), Is.False, "нашли запись обновления в воронеже");
			Assert.That(results.Any(r => r.Id == update2.Id), Is.True, "не нашли запись обновления в челябинске");
		}
	}
}