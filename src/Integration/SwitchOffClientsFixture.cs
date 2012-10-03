using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class SwitchOffClientsFixture : IntegrationFixture
	{
		[Test]
		public void Base_filter_test()
		{
			var filter = new SwitchOffClientsFilter { SortBy = "ClientName" };
			QueryCatcher.Catch();
			filter.Find(session);
		}
	}
}
