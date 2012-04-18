using System;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web.Hosting;
using AddUser;
using AdminInterface.Models.Security;
using CassiniDev;
using Functional.ForTesting;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;
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

			InitLogger(/*"NHibernate"*/);
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

		private void InitLogger()
		{
/*			var repository = LogManager.GetRepository();
			var hierarchy = ((Hierarchy)repository);
			hierarchy.Root.Level = Level.Debug;

			PatternLayout layout = new PatternLayout();
			layout.ConversionPattern = PatternLayout.DetailConversionPattern;
			layout.ActivateOptions();

			// Create the appender
			ConsoleAppender appender = new ConsoleAppender();
			appender.Layout = layout;
			appender.ActivateOptions();
			hierarchy.Root.AddAppender(new ConsoleAppender());
			hierarchy.Configured = true;*/
		}

		[TearDown]
		public void TeardownFixture()
		{
			_webServer.ShutDown();
		}
	}
}
