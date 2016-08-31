using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class UpdateLogSorting : AdmIntegrationFixture
	{
		[Test]
		public void SortColumns()
		{
			Client client = DataMother.CreateTestClientWithUser();
			IEnumerable<UpdateLogEntity> updateLog = new List<UpdateLogEntity>();

			for (int i = 0; i < 2; i++) {
				updateLog = updateLog.Concat(new[] { new UpdateLogEntity(client.Users.First()) {
						RequestTime = DateTime.Now.AddHours(-i*2),
						UpdateType = (UpdateType)i,
						ResultSize = (uint)i,
						AppVersion = (uint)i,
						Addition = String.Format("Test update{0}", i),
						Commit = false,
					}
				});
			}

			session.SaveEach(updateLog);
			Flush();

			var filter = new UpdateFilter();

			filter.Client = client;
			TrySortDate(filter);
		}

		private void TrySortDate(UpdateFilter filter, bool sum = false)
		{
			filter.BeginDate = DateTime.Now.Date.AddDays(-4);
			var logs = filter.Find(session);
			var count = logs.Count - 1;
			if (filter.SortDirection == "Desc") {
				for (int i = 0; i < count; i++) {
					Assert.That(logs[i].RequestTime, Is.GreaterThanOrEqualTo(logs[i++].RequestTime));
				}
			} else {
				for (int i = 0; i < count; i++) {
					Assert.That(logs[i].RequestTime, Is.LessThanOrEqualTo(logs[i++].RequestTime));
				}
			}
			if (!sum) {
				filter.SortDirection = filter.SortDirection == "Desc" ? "Asc" : "Desc";
				TrySortDate(filter, true);
			}
		}
	}
}
