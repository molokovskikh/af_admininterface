using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using AdminInterface.Models.Security;
using CassiniDev;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
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

			SecurityContext.GetAdministrator = () => admin;

			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();

			SetupEnvironment();

			Settings.Instance.AutoMoveMousePointerToTopLeft = false;
			Settings.Instance.MakeNewIeInstanceVisible = false;
			Settings.Instance.AutoCloseDialogs = true;

			if (Debugger.IsAttached)
			{
				Settings.Instance.WaitForCompleteTimeOut = int.MaxValue;
			}
		}

		private void SetupEnvironment()
		{
			var method = _webServer.GetType().GetMethod("GetHost", BindingFlags.Instance | BindingFlags.NonPublic);
			method.Invoke(_webServer, null);

			var manager = ApplicationManager.GetApplicationManager();
			var apps = manager.GetRunningApplications();
			var domain = manager.GetAppDomain(apps.Single().ID);
			domain.SetData("environment", "test");
		}

		[TearDown]
		public void TeardownFixture()
		{
			_webServer.ShutDown();
		}
	}
}
