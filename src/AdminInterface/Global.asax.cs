using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
#if !DEBUG
using System.Text;
using AdminInterface.Security;
#endif
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using System.Reflection;
using Common.Web.Ui.Models;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Type;

namespace AddUser
{
	public class Global : HttpApplication
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Global));

		void Application_Start(object sender, EventArgs e)
		{
			try
			{
				XmlConfigurator.Configure();
				GlobalContext.Properties["Version"] = Assembly.GetExecutingAssembly().GetName().Version;
				ActiveRecordStarter.Initialize(new[]
				                               	{
				                               		Assembly.Load("AdminInterface"),
				                               		Assembly.Load("Common.Web.Ui")
				                               	},
				                               ActiveRecordSectionHandler.Instance);

				SiteMap.Providers["SiteMapProvider"].SiteMapResolve += SiteMapResolve;

				var configuration = ActiveRecordMediator
					.GetSessionFactoryHolder()
					.GetAllConfigurations()[0];

				configuration.FilterDefinitions.Add("HomeRegionFilter",
				                                    new FilterDefinition("HomeRegionFilter",
				                                                         "",
				                                                         new Dictionary<string, IType>{{"AdminRegionMask", NHibernateUtil.UInt64}}));
				configuration.FilterDefinitions.Add("DrugstoreOnlyFilter",
													new FilterDefinition("DrugstoreOnlyFilter", "", new Dictionary<string, IType>()));
				configuration.FilterDefinitions.Add("SupplierOnlyFilter",
				                                    new FilterDefinition("SupplierOnlyFilter", "", new Dictionary<string, IType>()));

				var classMapping = configuration.GetClassMapping(typeof (Payer));
				var colection = (Collection) classMapping.GetProperty("Clients").Value;

				colection.AddFilter("HomeRegionFilter", "RegionCode & :AdminRegionMask > 0");
				colection.AddFilter("DrugstoreOnlyFilter", "FirmType = 1");
				colection.AddFilter("SupplierOnlyFilter", "FirmType = 0");
			}
			catch(Exception ex)
			{
				_log.Fatal("Ошибка при запуске Административного интерфеса", ex);
			}
		}

		private SiteMapNode SiteMapResolve(object sender, SiteMapResolveEventArgs e)
		{
			var currentNode = e.Provider.CurrentNode.Clone(true);
			if (currentNode.Url.EndsWith("/manageret.aspx"))
				currentNode.ParentNode.Url += "?cc=" + e.Context.Request["cc"];
			else if (currentNode.Url.EndsWith("/managep.aspx"))
				currentNode.ParentNode.Url += "?cc=" + e.Context.Request["cc"];
			else if (currentNode.Url.EndsWith("/EditRegionalInfo.aspx"))
			{
				uint firmCode;
				using (var connection = new MySqlConnection(Literals.GetConnectionString()))
				{
					connection.Open();
					var command = new MySqlCommand(@"
SELECT FirmCode
FROM usersettings.regionaldata rd
WHERE RowID = ?Id", connection);
					command.Parameters.AddWithValue("?Id", Convert.ToUInt32(e.Context.Request["id"]));
					firmCode = Convert.ToUInt32(command.ExecuteScalar());
				}
				currentNode.ParentNode.Url += "?cc="+firmCode;
				currentNode.ParentNode.ParentNode.Url += "?cc=" + firmCode;
			}
			else if (currentNode.Url.EndsWith("/managecosts.aspx"))
			{
				uint firmCode;
				using (var connection = new MySqlConnection(Literals.GetConnectionString()))
				{
					connection.Open();
					var command = new MySqlCommand(@"
SELECT FirmCode
FROM usersettings.PricesData pd
WHERE PriceCode = ?Id", connection);
					command.Parameters.AddWithValue("?Id", Convert.ToUInt32(e.Context.Request["pc"]));
					firmCode = Convert.ToUInt32(command.ExecuteScalar());
				}
				currentNode.ParentNode.Url += "?cc=" + firmCode;
				currentNode.ParentNode.ParentNode.Url += "?cc=" + firmCode;				
			}
			return currentNode;
		}

		void Session_Start(object sender, EventArgs e)
		{}

		void Application_BeginRequest(object sender, EventArgs e)
		{}

		void Application_AuthenticateRequest(object sender, EventArgs e)
		{}

		void Application_Error(object sender, EventArgs e)
		{
#if !DEBUG
			var exception = Server.GetLastError();

			if (exception.InnerException is NotAuthorizedException)
			{
				Response.Redirect("~/Rescue/NotAuthorized.aspx");
				return;
			}
			if (exception.InnerException is NotHavePermissionException)
			{
				Response.Redirect("~/Rescue/NotAllowed.aspx");
				return;
			}
			if (exception.InnerException is SessionOutDateException)
			{
				Response.Redirect("~/default.aspx");
				return;
			}

			var builder = new StringBuilder();
			builder.AppendLine("----Url-------");
			builder.AppendLine(Request.Url.ToString());
			builder.AppendLine("--------------");
			builder.AppendLine("----Params----");
			foreach (string name in Request.QueryString)
				builder.AppendLine(String.Format("{0}: {1}", name, Request.QueryString[name]));
			builder.AppendLine("--------------");
			
			builder.AppendLine("----Error-----");
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
					builder.AppendLine(String.Format("{0} - {1}", key, Session[key]));
			}
			builder.AppendLine("--------------");

			_log.Error(builder.ToString());
			Response.Redirect("~/Rescue/Error.aspx");
#endif
		}

		void Session_End(object sender, EventArgs e)
		{}

		void Application_End(object sender, EventArgs e)
		{}
	}
}