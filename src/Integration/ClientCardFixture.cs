using System.Linq;
using AddUser;
using AdminInterface.Helpers;
using Integration.ForTesting;
using Integration.Models;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class ClientCardFixture : IntegrationFixture
	{
		[Test]
		public void Send_client_card()
		{
			Global.Config.DocsPath = "../../../AdminInterface/Docs/";
			var client = DataMother.CreateTestClientWithUser();
			Flush();

			ReportHelper.SendClientCard(client.Users.First(), "", true, "kvasovtest@analit.net");
		}
	}
}