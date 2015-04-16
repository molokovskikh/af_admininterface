using System;
using System.IO;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Security;
using System.Reflection;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using log4net;

namespace AdminInterface
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
		public string ErrorFilesPath { get; set; }
		public string RedmineUrl { get; set; }
		public string RedmineAssignedTo { get; set; }
		public string NewSupplierMailFilePath { get; set; }
		public string NewSupplierAttachmentsPath { get; set; }
	}

	public class Global : WebApplication
	{
		public static AppConfig Config = new AppConfig();

		public Global()
			: base(Assembly.Load("AdminInterface"))
		{
			LibAssemblies.Add(Assembly.Load("Common.Web.Ui"));
			InstallBundle("jquery.validate");
			Logger.ErrorSubject = "Ошибка в Административном интерфейсе";
			Logger.SmtpHost = "box.analit.net";
		}

		private void Application_Start(object sender, EventArgs e)
		{
			try {
				BaseRemoteRequest.Runner = new WebRequestRunner();
				ConfigReader.LoadSettings(Config);
				Initialize();
				MixedRouteHandler.ConfigRoute();
			}
			catch (Exception ex) {
				Log.Fatal("Ошибка при запуске Административного интерфейса", ex);
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
	}
}