using System;
using System.Configuration;
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
					AdminInterface.Helpers.Printer.SetPrinter(printer);

					var brail = StandaloneInitializer.Init();

					if (args[1] == "invoice")
					{
						PrintInvoice(args, brail);
					}
					else if (args[1] == "act")
					{
						var ids = args[3].Split(',').Select(id => Convert.ToUInt32(id.Trim())).ToList();

						PrintActs(brail, ids);
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

		private static void PrintActs(IViewEngineManager brail, IEnumerable<uint> ids)
		{
			using (new SessionScope(FlushAction.Never))
			{
				foreach (var id in ids)
				{
					var act = Act.Find(id);
					new AdminInterface.Helpers.Printer().PrintView(brail,
						"Acts/Print",
						"Print",
						new Dictionary<string, object> {
							{ "act", act },
							{ "doc", act }
						});
				}
			}
		}

		private static void PrintInvoice(string[] args, IViewEngineManager brail)
		{
			var period = (Period)Period.Parse(typeof(Period), args[3]);
			var regionId = ulong.Parse(args[4]);
			var date = DateTime.Parse(args[5]);
			var recipientId = uint.Parse(args[6]);
			var invoicePeriod = Invoice.GetInvoicePeriod(period);

			using (new SessionScope(FlushAction.Never))
			{
				var region = Region.Find(regionId);
				var payers = ActiveRecordLinqBase<Payer>
					.Queryable
					.Where(p => p.AutoInvoice == InvoiceType.Auto
						&& p.PayCycle == invoicePeriod
							&& p.Recipient != null
								&& p.Recipient.Id == recipientId);

				foreach (var payer in payers)
				{
					if (!payer.Clients.Any(c => c.HomeRegion == region))
						continue;

					if (Invoice.Queryable.Any(i => i.Payer == payer && i.Period == period))
						continue;

					var invoice = new Invoice(payer, period, date);
					if (payer.InvoiceSettings.PrintInvoice)
					{
						using (var scope = new TransactionScope(OnDispose.Rollback))
						{
							payer.Balance -= invoice.Sum;
							invoice.Save();
							scope.VoteCommit();
						}

						new AdminInterface.Helpers.Printer().PrintView(brail,
							"Invoices/Print",
							"Print",
							new Dictionary<string, object> {
								{ "invoice", invoice },
								{ "doc", invoice }
							});
					}
				}
			}
		}
	}
}
