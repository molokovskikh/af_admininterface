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
	public class UpdateLogSorting : AdmSeleniumFixture
	{
		[Test]
		public void SortColumns()
		{
			Client client = DataMother.CreateTestClientWithUser();
			IEnumerable<UpdateLogEntity> updateLog = new List<UpdateLogEntity>();

			for (int i = 0; i < 2; i++)
			{
				updateLog = updateLog.Concat(new[] { new UpdateLogEntity(client.Users.First())
					{
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
			CommitAndContinue();

			Open(client);
			Click("История обновлений");
			OpenedWindow(String.Format("История обновлений клиента {0}", client.Name));

			var dates = browser.FindElementsByCssSelector("td[class='NotCommitedUpdate']");
			Assert.That(dates.First().Text, Is.GreaterThanOrEqualTo(dates.ElementAt(1).Text));

			Click("Дата");
			dates = browser.FindElementsByCssSelector("td[class='NotCommitedUpdate']");
			Assert.That(dates.First().Text, Is.LessThanOrEqualTo(dates.ElementAt(1).Text));
		}
	}
}