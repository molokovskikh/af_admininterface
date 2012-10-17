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

			var logBuilder = new StringBuilder();
			logBuilder.AppendLine(client.Settings.ServiceClient.ToString());
			logBuilder.AppendLine(client.Settings.InvisibleOnFirm.GetDescription());
			logBuilder.AppendLine(client.HomeRegion.Id.ToString());
			logBuilder.AppendLine(SecurityContext.Administrator.RegionMask.ToString());
			logBuilder.AppendLine(filter.Region.Id.ToString());
			File.WriteAllText("AnalysisOfWorkDrugstoresFixture.txt", logBuilder.ToString(), Encoding.UTF8);

			Flush();

			var result = filter.Find();
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.IsTrue(result.Any(r => r.Id == client.Id));

			logBuilder.AppendLine(result.Implode());
			logBuilder.AppendLine(client.Id.ToString());

			File.WriteAllText("AnalysisOfWorkDrugstoresFixture.txt", logBuilder.ToString(), Encoding.UTF8);
		}
	}
}
