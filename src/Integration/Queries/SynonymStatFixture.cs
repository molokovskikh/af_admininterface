using System;
using System.Linq;
using System.Linq.Expressions;
using AdminInterface.Queries;
using NUnit.Framework;
using Test.Support;

namespace Integration.Queries
{
	[TestFixture]
	public class SynonymStatFixture : IntegrationFixture
	{
		[Test]
		public void Find()
		{
			session.CreateSQLQuery("insert into Logs.SynonymLogs(LogTime, OperatorName, OperatorHost, Operation)" +
				" values(now(), 'test-log', 'localhost', 0)")
				.ExecuteUpdate();
			session.CreateSQLQuery("insert into Logs.SynonymFirmCrLogs(LogTime, OperatorName, OperatorHost, Operation)" +
				" values(now(), 'test-log', 'localhost', 0)")
				.ExecuteUpdate();
			var query = new SynonymStat();
			query.Period.Begin = DateTime.Today;
			query.Period.End = DateTime.Today;
			var stats = query.Find(session);
			Assert.That(stats.Count, Is.GreaterThan(0));
			Assert.AreEqual(1, stats.Count(s => s.OperatorName == "test-log"));
		}
	}
}