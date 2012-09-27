using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	public class ManagerReportFixture : IntegrationFixture
	{
		private Client _client;
		[SetUp]
		public void SetUp()
		{
			var log = new RejectWaybillLog();
			_client = DataMother.CreateTestClientWithAddress();
			log.ForClient = _client;
			log.LogTime = DateTime.Now;
			Save(log);
			Flush();
		}
		[Test]
		public void ClientAddressFilterTest()
		{
			var filter = new ClientAddressFilter();
			filter.Find();

			var results = filter.Find();
			Assert.That(results.Count(t => t.ClientId == _client.Id), Is.GreaterThan(0));
		}
	}
}
