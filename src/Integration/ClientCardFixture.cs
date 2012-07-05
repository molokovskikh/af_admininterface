using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class ClientCardFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Send_client_card()
		{
			var client = DataMother.CreateTestClientWithUser();
			var defaults = session.Query<DefaultValues>().First();
			Flush();

			ReportHelper.SendClientCard(client.Users.First(), "", true,  defaults, "kvasovtest@analit.net");
		}
	}
}