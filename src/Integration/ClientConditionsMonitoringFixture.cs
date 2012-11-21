using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using Common.Web.Ui.Models;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class ClientConditionsMonitoringFixture : IntegrationFixture
	{
		[Test]
		public void Base_filter_work_test()
		{
			var region = session.Query<Region>().FirstOrDefault(r => r.Name == "Воронеж");
			var filter = new ClientConditionsMonitoringFilter { Session = session, Region = region, ClientId = 2136 };
			filter.Find();
		}
	}
}
