using System;
using System.Configuration;
using System.IO;
using AdminInterface.Models.Security;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.VisualStudio.WebHost;
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
			ForTest.InitialzeAR();
			SecurityContext.GetAdministrator = () => new Administrator{UserName = "test"};

			var port = int.Parse(ConfigurationManager.AppSettings["webPort"]);
			var webDir = ConfigurationManager.AppSettings["webDirectory"];

			_webServer = new Server(port, "/", Path.GetFullPath(webDir));
			_webServer.Start();
			Settings.Instance.AutoMoveMousePointerToTopLeft = false;
			Settings.Instance.MakeNewIeInstanceVisible = false;

			InitLogger(/*"NHibernate"*/);
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
			_webServer.Stop();
		}
	}
}
