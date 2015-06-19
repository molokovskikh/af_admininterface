using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;

namespace Integration
{
	public class ManagerReportFixture : AdmIntegrationFixture
	{
		private Client _client;

		[SetUp]
		public void SetUp()
		{
			var log = new RejectWaybillLog();
			_client = DataMother.CreateTestClientWithAddress();
			log.ForClient = session.Get<ClientForReading>(_client.Id);
			log.LogTime = DateTime.Now;
			Save(log);
			Flush();
		}

		[Test]
		public void ClientAddressFilterTest()
		{
			var filter = new ClientAddressFilter { Period = new DatePeriod(DateTime.Now.AddDays(-14), DateTime.Now) };
			filter.Session = session;
			var results = filter.Find();
			Assert.That(results.Count(t => t.ClientId == _client.Id), Is.GreaterThan(0));
		}
	}
}
