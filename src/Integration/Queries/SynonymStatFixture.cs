using System;
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
				" values(now(), 'test', 'localhost', 0)")
				.ExecuteUpdate();
			var query = new SynonymStat();
			query.Period.Begin = DateTime.Today;
			query.Period.End = DateTime.Today;
			var stats = query.Find(session);
			Assert.That(stats.Count, Is.GreaterThan(0));
		}
	}
}