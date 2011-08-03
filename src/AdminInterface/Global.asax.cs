using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Initializers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using System.Reflection;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Container;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Views.Aspx;
using Castle.MonoRail.Views.Brail;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using log4net;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using ILoggerFactory = Castle.Core.Logging.ILoggerFactory;

namespace AddUser
{
	public class Global : WebApplication, IMonoRailConfigurationEvents, IMonoRailContainerEvents
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Global));

		public Global()
			: base(Assembly.Load("AdminInterface"))
		{
			LibAssemblies.Add(Assembly.Load("Common.Web.Ui"));
			Logger.ErrorSubject = "Ошибка в Административном интерфейсе";
			Logger.SmtpHost = "box.analit.net";
		}

		void Application_Start(object sender, EventArgs e)
		{
			try
			{
				Initialize();
				SetupRoute();
			}
			catch(Exception ex)
			{
				_log.Fatal("Ошибка при запуске Административного интерфеса", ex);
			}
		}

		public class BugRoute : IRoutingRule
		{
			private PatternRoute route;

			public BugRoute(PatternRoute route)
			{
				this.route = route;
			}

			public string CreateUrl(IDictionary parameters)
			{
				return route.CreateUrl(parameters);
			}

			public int Matches(string url, IRouteContext context, RouteMatch match)
			{
				if (url.Contains("WebResource.axd"))
					return 0;

				return route.Matches(url, context, match);
			}

			public string RouteName
			{
				get { return route.RouteName; }
			}
		}

		private void SetupRoute()
		{
			var engine = RoutingModuleEx.Engine;

			engine.Add(
				new PatternRoute("/<controller>/<id>")
					.DefaultForAction().Is("show")
					.Restrict("id").ValidInteger
			);

			engine.Add(
				new BugRoute(
					new PatternRoute("/<controller>/[action]")
						.DefaultForAction().Is("index")
				)
			);

			engine.Add(
				new PatternRoute("/<controller>/[id]/<action>")
					.Restrict("id")
					.ValidInteger
			);

			engine.Add(new PatternRoute("/client/[clientId]/orders")
				.DefaultForController().Is("Logs")
				.DefaultForAction().Is("Orders")
				.Restrict("clientId").ValidInteger);

			engine.Add(new PatternRoute("/deliveries/[id]/edit")
				.DefaultForController().Is("deliveries")
				.DefaultForAction().Is("edit")
				.Restrict("id").ValidInteger);

			engine.Add(new PatternRoute("/users/search")
				.DefaultForController().Is("UserSearch")
				.DefaultForAction().Is("Search"));

			engine.Add(new PatternRoute("/")
				.DefaultForController().Is("Main")
				.DefaultForAction().Is("Index"));

			engine.Add(new PatternRoute("default.aspx")
				.DefaultForController().Is("Main")
				.DefaultForAction().Is("Index"));
		}

		private SiteMapNode SiteMapResolve(object sender, SiteMapResolveEventArgs e)
		{
			var currentNode = e.Provider.CurrentNode.Clone(true);
			if (currentNode.Url.EndsWith("/manageret.aspx"))
				currentNode.ParentNode.Url += e.Context.Request["cc"];
			else if (currentNode.Url.EndsWith("/managep.aspx"))
				currentNode.ParentNode.Url += e.Context.Request["cc"];
			else if (currentNode.Url.EndsWith("/SenderProperties.aspx"))
			{
				uint firmCode;
				using (var connection = new MySqlConnection(Literals.GetConnectionString()))
				{
					connection.Open();
					var command = new MySqlCommand(@"
select firmcode 
from ordersendrules.order_send_rules osr
where osr.id = ?ruleId
", connection);
					command.Parameters.AddWithValue("?RuleId", e.Context.Request["RuleId"]);
					firmCode = Convert.ToUInt32(command.ExecuteScalar());
				}
				currentNode.ParentNode.Url += "?cc=" + firmCode;
				currentNode.ParentNode.ParentNode.Url += firmCode;
			}
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
				currentNode.ParentNode.Url += "?cc=" + firmCode;
				currentNode.ParentNode.ParentNode.Url += firmCode;
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
				currentNode.ParentNode.ParentNode.Url += firmCode;
			}
			return currentNode;
		}

		void Session_Start(object sender, EventArgs e)
		{}

		void Application_BeginRequest(object sender, EventArgs e)
		{}

		void Application_AuthenticateRequest(object sender, EventArgs e)
		{}

		void Session_End(object sender, EventArgs e)
		{}

		void Application_End(object sender, EventArgs e)
		{}

		void Application_Error(object sender, EventArgs e)
		{
			var exception = Server.GetLastError();
			if (exception.InnerException is NotAuthorizedException)
			{
				Server.Transfer("~/Rescue/NotAuthorized.aspx");
				return;
			}
			if (exception.InnerException is NotHavePermissionException)
			{
				Server.Transfer("~/Rescue/NotAllowed.aspx");
				return;
			}

			if (!Context.IsDebuggingEnabled)
				Server.Transfer("~/Rescue/Error.aspx");
		}

		public new void Initialized(IMonoRailContainer container)
		{
			BaseMailer.ViewEngineManager = container.ViewEngineManager;

			var monorailContainer = ((DefaultMonoRailContainer)container);
			var builder = new UrlBuilder();
			monorailContainer.ServiceInitializer.Initialize(builder, container);
			monorailContainer.AddService<IUrlBuilder>(builder);

			base.Initialized(container);
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