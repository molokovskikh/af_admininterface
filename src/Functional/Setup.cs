using AdminInterface.Models.Security;
using CassiniDev;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using SecurityContext = AdminInterface.Security.SecurityContext;

namespace Functional
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[SetUp]
		public void SetupFixture()
		{
			Test.Support.IntegrationFixture.DoNotUserTransaction = true;

			ForTest.InitialzeAR();

			var admin = Administrator.CreateLocalAdministrator();
			ActiveRecordMediator.Save(admin);
			SecurityContext.GetAdministrator = () => admin;

			_webServer = WatinSetup.StartServer();
		}

		[TearDown]
		public void TeardownFixture()
		{
			_webServer.ShutDown();
		}
	}
}