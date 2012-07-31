using AdminInterface.Queries;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration.Queries
{
	[TestFixture]
	public class ResetReclameDateFixture : IntegrationFixture
	{
		[Test]
		public void Reset_reclame_date()
		{
			var client = DataMother.CreateClientAndUsers();
			new ResetReclameDate(client).Execute(session);
		}
	}
}