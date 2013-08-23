using CassiniDev;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Selenium;
using Test.Support.Web;

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
			_webServer = WatinSetup.StartServer();
			SeleniumFixture.GlobalSetup();
		}

		[TearDown]
		public void TeardownFixture()
		{
			_webServer.ShutDown();
			SeleniumFixture.GlobalTearDown();
		}
	}
}