using System;
using System.Linq;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using Integration.Models;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class ClientCardFixture : IntegrationFixture
	{
		[Test]
		public void t()
		{
			var client = DataMother.CreateClientAndUsers();

			session.Clear();

			Console.WriteLine(session.Get<Service>(client.Id));
		}

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