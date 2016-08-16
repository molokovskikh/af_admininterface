using System;
using CassiniDev;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using Test.Support.Selenium;
using Test.Support.Web;

namespace Functional
{
	[SetUpFixture]
	public class Setup
	{
		private Server _webServer;

		[OneTimeSetUp]
		public void SetupFixture()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
			ForTest.InitialzeAR();
			IntegrationFixture2.Factory = ActiveRecordMediator.GetSessionFactoryHolder()
				.GetSessionFactory(typeof(ActiveRecordBase));
			_webServer = WatinSetup.StartServer();
			SeleniumFixture.WebPort = WatinSetup.WebPort;
			SeleniumFixture.GlobalSetup();
		}

		[OneTimeTearDown]
		public void TeardownFixture()
		{
			_webServer.ShutDown();
			SeleniumFixture.GlobalTearDown();
			WatinFixture2.GlobalCleanup();
		}
	}
}