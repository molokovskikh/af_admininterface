using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Test;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	[TestFixture]
	public class BaseFiltersFixture : AdmIntegrationFixture
	{
		[Test]
		public void Base_filter_test()
		{
			var filter = new SwitchOffClientsFilter { SortBy = "ClientName" };
			filter.Find(session);
		}
	}
}
