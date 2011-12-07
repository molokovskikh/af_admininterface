using System;
using System.Collections;
using System.Configuration;
using System.Threading;
using AdminInterface.Helpers;
using Castle.ActiveRecord.Framework.Internal;
using Castle.MonoRail.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Common.Tools;
using log4net;
using log4net.Config;

namespace Printer
{
	class Program
	{
		static void Main(string[] args)
		{
			//сохранение счетов\актов производится внутри транзации что бы она гарантировано
			//закончилась и акты стали доступны нужно немного подождать
			Thread.Sleep(1000);

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
					IEnumerable documents = null;
					using(new SessionScope(FlushAction.Never))
					{
						if (name == "invoice")
						{
							documents = Invoice.Queryable.Where(a => ids.Contains(a.Id))
								.OrderBy(a => a.PayerName)
								.ToArray();
						}
						else if (name == "act")
						{
							documents = Act.Queryable.Where(a => ids.Contains(a.Id))
								.OrderBy(a => a.PayerName)
								.ToArray();
						}
						Print(brail, name, documents);
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

		private static void Print(IViewEngineManager brail, string name, IEnumerable documents)
		{
			var plural = Inflector.Pluralize(name);
			if (String.IsNullOrEmpty(plural))
				plural = name;
			var singular = Inflector.Singularize(name);
			if (String.IsNullOrEmpty(singular))
				singular = name;

			using (new SessionScope(FlushAction.Never))
			{
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
