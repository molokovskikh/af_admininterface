using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using System.Reflection;
using Castle.ActiveRecord.Framework.Internal;
using Castle.Components.Validator;
using Castle.Core.Configuration;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Configuration;
using Castle.MonoRail.Framework.Container;
using Castle.MonoRail.Framework.Descriptors;
using Castle.MonoRail.Framework.Helpers.ValidationStrategy;
using Castle.MonoRail.Framework.Internal;
using Castle.MonoRail.Framework.JSGeneration;
using Castle.MonoRail.Framework.JSGeneration.jQuery;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Views.Aspx;
using Castle.MonoRail.Views.Brail;
using log4net;
using log4net.Config;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Type;

namespace AddUser
{
	public class Global : HttpApplication, IMonoRailConfigurationEvents, IMonoRailContainerEvents
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (Global));

		void Application_Start(object sender, EventArgs e)
		{
			try
			{
				XmlConfigurator.Configure();
				GlobalContext.Properties["Version"] = Assembly.GetExecutingAssembly().GetName().Version;

				var types = Assembly.GetExecutingAssembly().GetTypes()
					.Where(t => t.Namespace != null && t.Namespace.EndsWith(".Initializers"));

				foreach (var type in types)
				{
					if (type.GetMethod("Initialize") == null)
						continue;

					var initializer = (dynamic)Activator.CreateInstance(type);
					initializer.Initialize(ActiveRecordSectionHandler.Instance);
				}

				SiteMap.Providers["SiteMapProvider"].SiteMapResolve += SiteMapResolve;

				SetupSecurityFilters();

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

			engine.Add(new PatternRoute("/client/[cc]")
				.DefaultForController().Is("client")
				.DefaultForAction().Is("info")
				.Restrict("cc").ValidInteger);

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
					.Restrict("id").ValidInteger
			);

			engine.Add(new PatternRoute("/client/[clientId]/orders")
				.DefaultForController().Is("Logs")
				.DefaultForAction().Is("Orders")
				.Restrict("clientId").ValidInteger);

			engine.Add(new PatternRoute("/deliveries/[id]/edit")
				.DefaultForController().Is("deliveries")
				.DefaultForAction().Is("edit")
				.Restrict("id").ValidInteger);
		
			engine.Add(new PatternRoute("/users/[id]/settings")
				.DefaultForController().Is("users")
				.DefaultForAction().Is("UserSettings"));
			
			engine.Add(new PatternRoute("/users/search")
				.DefaultForController().Is("UserSearch")
				.DefaultForAction().Is("Search"));

			engine.Add(new PatternRoute("/RegionalAdmin/[id]/edit")
				.DefaultForController().Is("RegionalAdmin")
				.DefaultForAction().Is("Edit"));
			
			engine.Add(new PatternRoute("/")
				.DefaultForController().Is("Main")
				.DefaultForAction().Is("Index"));

			engine.Add(new PatternRoute("default.aspx")
				.DefaultForController().Is("Main")
				.DefaultForAction().Is("Index"));
		}

		private void SetupSecurityFilters()
		{
			var configuration = ActiveRecordMediator
				.GetSessionFactoryHolder()
				.GetAllConfigurations()[0];

			configuration.FilterDefinitions.Add("RegionFilter",
				new FilterDefinition("RegionFilter",
					"",
					new Dictionary<string, IType> {{"AdminRegionMask", NHibernateUtil.UInt64}},
					true));
			configuration.FilterDefinitions.Add("DrugstoreOnlyFilter",
				new FilterDefinition("DrugstoreOnlyFilter", "", new Dictionary<string, IType>(), true));
			configuration.FilterDefinitions.Add("SupplierOnlyFilter",
				new FilterDefinition("SupplierOnlyFilter", "", new Dictionary<string, IType>(), true));

			var payerMapping = configuration.GetClassMapping(typeof (Payer));
			var colection = (Collection) payerMapping.GetProperty("Clients").Value;

			colection.AddFilter("RegionFilter", "RegionCode & :AdminRegionMask > 0");
			colection.AddFilter("DrugstoreOnlyFilter", "FirmType = 1");
			colection.AddFilter("SupplierOnlyFilter", "FirmType = 0");

			var regionMapping = configuration.GetClassMapping(typeof (Region));
			regionMapping.AddFilter("RegionFilter", "RegionCode & :AdminRegionMask > 0");
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

		void Application_Error(object sender, EventArgs e)
		{
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

			var builder = new StringBuilder();
			builder.AppendLine("----UrlReferer-------");
			builder.AppendLine(Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : String.Empty);
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
			try
			{
				foreach (string key in Session.Keys)
				{
					if (Session[key] == null)
						builder.AppendLine(String.Format("{0} - null", key));
					else
						builder.AppendLine(String.Format("{0} - {1}", key, Session[key]));
				}
			}
			catch (Exception)
			{}
			builder.AppendLine("--------------");

			_log.Error(builder.ToString());
#if !DEBUG
			Response.Redirect("~/Rescue/Error.aspx");
#endif
		}

		void Session_End(object sender, EventArgs e)
		{}

		void Application_End(object sender, EventArgs e)
		{}

		public void Configure(IMonoRailConfiguration configuration)
		{
/*
			configuration.JSGeneratorConfiguration.AddLibrary("jquery", typeof(JQueryGenerator))
				.AddExtension(typeof(CommonJSExtension))
				.ElementGenerator
					.AddExtension(typeof(JQueryElementGenerator))
					.Done
				.BrowserValidatorIs(typeof(JQueryValidator))
				.SetAsDefault();
 */

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

#if DEBUG
			MonoRail.Debugger.Toolbar.Toolbar.Init(configuration);
#endif

/*
			configuration.SmtpConfig.Host = "mail.adc.analit.net";
			configuration.ExtensionEntries.Add(new ExtensionEntry(typeof(ExceptionChainingExtension),
				new MutableConfiguration("mailTo")));
*/
		}

		public void Created(IMonoRailContainer container)
		{}

		public void Initialized(IMonoRailContainer container)
		{
			container.ValidatorRegistry = new CachedValidationRegistry(new ResourceManager("Castle.Components.Validator.Messages", typeof(CachedValidationRegistry).Assembly));
			container.ControllerDescriptorProvider.AfterProcess += desc => {
				if (desc.Helpers.Any(d => d.HelperType == typeof(AppHelper)))
					return;
				desc.Helpers = desc.Helpers.Concat(new [] {new HelperDescriptor(typeof(AppHelper), "app"), }).ToArray();
			};
			BaseMailer.ViewEngineManager = container.ViewEngineManager;
			((DefaultViewComponentFactory)container.GetService<IViewComponentFactory>()).Inspect(Assembly.Load("AdminInterface"));
			((DefaultViewComponentFactory)container.GetService<IViewComponentFactory>()).Inspect(Assembly.Load("Common.Web.Ui"));
			container.UrlBuilder.UseExtensions = false;
		}
	}
}