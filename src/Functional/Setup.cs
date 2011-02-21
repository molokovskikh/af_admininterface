using System.IO;
using System.Configuration;
using System.Collections.Generic;
using AdminInterface.Models.Security;
using CassiniDev;
using Functional.ForTesting;
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
			ForTest.InitialzeAR();
			var admin = new Administrator{
				UserName = "test",
				Email = "kvasovtest@analit.net",
				PhoneSupport = "112",
				RegionMask = ulong.MaxValue,
				ManagerName = "test",
				AllowedPermissions = new List<Permission> {
					Permission.Find(PermissionType.Billing),
					Permission.Find(PermissionType.ViewDrugstore),
				}
			};
			admin.Save();

			SecurityContext.GetAdministrator = () => admin;

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
			_webServer.ShutDown();
		}
	}
}
