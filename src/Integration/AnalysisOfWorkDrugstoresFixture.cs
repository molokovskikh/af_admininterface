using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.ManagerReportsFilters;
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

			Flush();

			var filter = new AnalysisOfWorkDrugstoresFilter(session);
			var result = filter.Find();
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.IsTrue(result.Select(r => r.Id).Contains(client.Id));
		}
	}
}
