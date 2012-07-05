using System.Linq;
using AdminInterface.Helpers;
using Integration.ForTesting;
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
			Flush();

			ReportHelper.SendClientCard(client.Users.First(), "", true, "kvasovtest@analit.net");
		}
	}
}