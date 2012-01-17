using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class RegistrationInformationFixture
	{
		[Test]
		public void BaseTest()
		{
			QueryCatcher.Catch();
			var filter = new UserFinderFilter();
			//filter.FinderType = RegistrationFinderType.Addresses;

			var criteria = filter.GetCriteria();

			filter.ApplySort(criteria);

			var client = ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).ToList<RegistrationInformation>()).ToList();
		}
	}
}
