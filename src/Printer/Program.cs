using System;
using System.Configuration;
using AdminInterface.Helpers;
using Castle.ActiveRecord.Framework.Internal;
using Castle.MonoRail.Framework;
using Common.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;
using log4net;
using log4net.Config;

namespace Printer
{
	class Program
	{
		static void Main(string[] args)
		{
			XmlConfigurator.Configure();
			var logger = LogManager.GetLogger(typeof(Program));
			try
			{
				if (args.FirstOrDefault() == "print")
				{
					var printer = args[2];
					var name = args[1];
					var ids = args[3].Split(',').Select(id => Convert.ToUInt32(id.Trim())).ToArray();

					InternetExplorerPrinterHelper.SetPrinter(printer);
					var brail = StandaloneInitializer.Init();
					if (name == "invoice")
					{
						Print(brail, name, Invoice.Queryable.Where(a => ids.Contains(a.Id)).OrderBy(a => a.PayerName));
					}
					else if (name == "act")
					{
						Print(brail, name, Act.Queryable.Where(a => ids.Contains(a.Id)).OrderBy(a => a.PayerName));
					}
				}
				else
				{
					var assembly = Assembly.GetExecutingAssembly();
					var exe = assembly.Location;
					var arguments = String.Join(" ", args.Select(a => { 
						if (!a.Contains(" "))
							return a;
						return "\"" + a + "\"";
					}));
#if DEBUG
					Process.Start(exe, "print " + arguments).WaitForExit();
#else
					ProcessStarter.StartProcessInteractivly(exe + " print " + arguments,
						ConfigurationManager.AppSettings["User"],
						ConfigurationManager.AppSettings["Password"] ,
						"analit");
#endif
				}
			}
			catch (Exception e)
			{
				logger.Error("ошибка", e);
			}
		}

		private static void Print<T>(IViewEngineManager brail, string name, IOrderedQueryable<T> query)
		{
			var plural = Inflector.Pluralize(name);
			if (String.IsNullOrEmpty(plural))
				plural = name;
			var singular = Inflector.Singularize(name);
			if (String.IsNullOrEmpty(name))
				singular = name;

			using (new SessionScope(FlushAction.Never))
			{
				var documents = query.ToArray();
				foreach (var document in documents)
				{
					new InternetExplorerPrinterHelper().PrintView(brail,
						Inflector.Capitalize(plural) + "/Print",
						"Print",
						new Dictionary<string, object> {
							{ singular, document },
							{ "doc", document }
						});
				}
			}
		}
	}
}
