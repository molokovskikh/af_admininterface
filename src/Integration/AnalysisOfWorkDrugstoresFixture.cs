using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework.Helpers;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.Test;
using Integration.ForTesting;
using NUnit.Framework;
using Rhino.Mocks;
using Test.Support;

namespace Integration
{
	[TestFixture]
	public class AnalysisOfWorkDrugstoresFixture : BaseHelperFixture
	{
		[Test]
		public void BaseTest()
		{
			var client = new Client();
			using (var scope = new SessionScope()) {
				ArHelper.WithSession(s => {
					client = DataMother.CreateClientAndUsers();
					s.Save(client);

					var filter = new AnalysisOfWorkDrugstoresFilter { Session = s, PagesSize = 1000 };
					var result = filter.Find();

					var urlHelper = new UrlHelper(context);
					PrepareHelper(urlHelper);
					urlHelper.SetController(new ManagerReportsController(), context.CurrentControllerContext);
					urlHelper.UrlBuilder = new DefaultUrlBuilder();

					foreach (var baseItemForTable in result) {
						baseItemForTable.SetUrlHelper(urlHelper);
					}
					Assert.That(result.Count, Is.GreaterThan(0));
					Assert.IsTrue(result.Any(r => ((dynamic)r).Id.Contains(client.Id.ToString())));
				});
			}
		}
	}
}
