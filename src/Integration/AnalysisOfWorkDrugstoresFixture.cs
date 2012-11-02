using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Security;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	[TestFixture]
	public class AnalysisOfWorkDrugstoresFixture : IntegrationFixture
	{
		[Test]
		public void BaseTest()
		{
			var client = DataMother.CreateClientAndUsers();
			session.Save(client);

			var filter = new AnalysisOfWorkDrugstoresFilter();
			filter.Session = session;

			Flush();

			filter.PagesSize = 1000;
			var result = filter.Find();
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.IsTrue(result.Any(r => ((dynamic)r).Id == client.Id));
		}
	}
}
