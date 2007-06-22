using System;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Runtime.InteropServices;
using ActiveDs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using DAL;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Reflection;

namespace AddUser
{
	public class Global : HttpApplication
	{
		private System.ComponentModel.IContainer components;
		
		public Global()
		{
			InitializeComponent();
		}
		
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}

		void Application_Start(object sender, EventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();
			ActiveRecordStarter.Initialize(Assembly.Load("AdminInterface"),
										   ActiveRecordSectionHandler.Instance);

		}

		void Session_Start(object sender, EventArgs e)
		{
			Session["strStatus"] = "No";
			Session["strError"] = "";
			string UserName;
			UserName = HttpContext.Current.User.Identity.Name;
			if (UserName.Substring(0, 7) == "ANALIT\\")
			#if DEBUG
				UserName = "michail";
			#else
				UserName = UserName.Substring(7);
			#endif

			Administrator administrator = CommandFactory.GetAdministrator(UserName);
			if (administrator != null)
				Session["Administrator"] = administrator;
			
			Session["UserName"] = UserName;
			Session["SessionID"] = Session.SessionID;
		}

		void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		void Application_Error(object sender, EventArgs e)
		{
#if !DEBUG
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("----Url-------");
			builder.AppendLine(Request.Url.ToString());
			builder.AppendLine("--------------");
			builder.AppendLine("----Params----");
			foreach (string name in Request.QueryString)
				builder.AppendLine(String.Format("{0}: {1}", name, Request.QueryString[name]));
			builder.AppendLine("--------------");
			
			builder.AppendLine("----Error-----");
			Exception exception = Server.GetLastError();
			do
			{
				builder.AppendLine("Message:");
				builder.AppendLine(exception.Message);
				builder.AppendLine("Stack Trace:");
				builder.AppendLine(exception.StackTrace);
				builder.AppendLine("--------------");
				exception = exception.InnerException;
			} while (exception != null);
			builder.AppendLine("--------------");

			builder.AppendLine("----Session---");
			foreach (string key in Session.Keys)
			{
				if (Session[key] == null)
					builder.AppendLine(String.Format("{0} - null", key));
				else
					builder.AppendLine(String.Format("{0} - {1}", key, Session[key].GetType().ToString()));
			}
			builder.AppendLine("--------------");

			builder.AppendLine(String.Format("Version : {0}", Assembly.GetExecutingAssembly().GetName().Version));
			Logger.Write(builder.ToString(), "Error");
#endif
		}

		void Session_End(object sender, EventArgs e)
		{
		}

		void Application_End(object sender, EventArgs e)
		{
		}
	}
}