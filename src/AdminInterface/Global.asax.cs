using System;
using System.IO;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using System.Reflection;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Container;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.Views.Aspx;
using Castle.MonoRail.Views.Brail;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using log4net;

namespace AddUser
{
	public class AppConfig : BaseConfig
	{
		public string ReportSystemPassword { get; set; }
		public string ReportSystemUser { get; set; }
		public string DeleteReportUri { get; set; }

		public string AptBox { get; set; }
		public string OptBox { get; set; }
		public string UserPreparedDataDirectory { get; set; }
		public string CallRecordsDirectory { get; set; }
		public string PromotionsPath { get; set; }

		public string PrinterPath { get; set; }
		public string DocsPath { get; set; }

		public string AttachmentsPath { get; set; }
	}

	public class Global : WebApplication, IMonoRailConfigurationEvents
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Global));

		public static AppConfig Config = new AppConfig();

		public Global()
			: base(Assembly.Load("AdminInterface"))
		{
			LibAssemblies.Add(Assembly.Load("Common.Web.Ui"));
			Logger.ErrorSubject = "Ошибка в Административном интерфейсе";
			Logger.SmtpHost = "box.analit.net";
			Logger.TimeOutAction = RedirectIfTimeOutException;
		}

		private void RedirectIfTimeOutException(HttpContext context)
		{
			context.Response.Redirect(Path.Combine(context.Request.ApplicationPath, "Error/TimeOutError"));
		}

		private void Application_Start(object sender, EventArgs e)
		{
			try {
				BaseRemoteRequest.Runner = new WebRequestRunner();
				ConfigReader.LoadSettings(Config);
				Initialize();
			}
			catch (Exception ex) {
				_log.Fatal("Ошибка при запуске Административного интерфеса", ex);
			}
		}

		private void Application_Error(object sender, EventArgs e)
		{
			var exception = Server.GetLastError();
			if (exception.InnerException is NotAuthorizedException) {
				Server.Transfer("~/Rescue/NotAuthorized.aspx");
				return;
			}
			if (exception.InnerException is NotHavePermissionException) {
				Server.Transfer("~/Rescue/NotAllowed.aspx");
				return;
			}

			if (!Context.IsDebuggingEnabled)
				Server.Transfer("~/Rescue/Error.aspx");
		}

		public new void Configure(IMonoRailConfiguration configuration)
		{
			configuration.ControllersConfig.AddAssembly("AdminInterface");
			configuration.ControllersConfig.AddAssembly("Common.Web.Ui");
			configuration.ViewComponentsConfig.Assemblies = new[] {
				"AdminInterface",
				"Common.Web.Ui"
			};
			configuration.ViewEngineConfig.ViewPathRoot = "Views";
			configuration.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(BooViewEngine), false));
			configuration.ViewEngineConfig.ViewEngines.Add(new ViewEngineInfo(typeof(WebFormsViewEngine), false));
			configuration.ViewEngineConfig.AssemblySources.Add(new AssemblySourceInfo("Common.Web.Ui", "Common.Web.Ui.Views"));
			configuration.ViewEngineConfig.VirtualPathRoot = configuration.ViewEngineConfig.ViewPathRoot;
			configuration.ViewEngineConfig.ViewPathRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.ViewEngineConfig.ViewPathRoot);

			base.Configure(configuration);
		}
	}
}