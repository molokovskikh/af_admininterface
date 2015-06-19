using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Queries;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Test;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AnalysisOfWorkDrugstoresFixture : BaseHelperFixture
	{
		[Test]
		public void BaseTest()
		{
			var client = new Client();
			IList<BaseItemForTable> result = new List<BaseItemForTable>();
			using (new SessionScope()) {
				ArHelper.WithSession(s => {
					client = new DataMother(s).CreateClientAndUsers();

					var filter = new AnalysisOfWorkDrugstoresFilter(1000);
					filter.Session = s;
					result = filter.Find();

					var appHelper = new AppHelper(context);
					PrepareHelper(appHelper);

					foreach (var baseItemForTable in result) {
						baseItemForTable.SetHelpers(appHelper);
					}
					Assert.That(result.Count, Is.GreaterThan(0));

					var clientIds = result.Select(r => ((AnalysisOfWorkFiled)r).Id).ToList();
					var Id = client.Id.ToString();
					Assert.IsTrue(clientIds.Any(r => r.Contains(Id)));
				});
			}
		}
	}
}