using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using Castle.ActiveRecord.Linq;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Brail;
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
					var period = (Period)Period.Parse(typeof(Period), args[1]);
					var regionId = ulong.Parse(args[2]);
					var date = DateTime.Parse(args[3]);
					AdminInterface.Helpers.Printer.SetPrinter(args[4]);

					ActiveRecordStarter.Initialize(new[] {
						Assembly.Load("AdminInterface"),
						Assembly.Load("Common.Web.Ui")
					},
						ActiveRecordSectionHandler.Instance);

					var options = new BooViewEngineOptions();
					options.AssembliesToReference.Add(typeof (Controller).Assembly);
					var loader = new FileAssemblyViewSourceLoader("");
					loader.AddAssemblySource(new AssemblySourceInfo(Assembly.GetExecutingAssembly(), "Printer"));
					var brail = new StandaloneBooViewEngine(loader, options);

					var invoicePeriod = Invoice.GetInvoicePeriod(period);

					using (new SessionScope(FlushAction.Never))
					{
						var region = Region.Find(regionId);
						var payers = ActiveRecordLinqBase<Payer>
							.Queryable
							.Where(p => p.AutoInvoice == InvoiceType.Auto && p.PayCycle == invoicePeriod);

						foreach (var payer in payers)
						{
							if (!payer.JuridicalOrganizations.Any(j => j.Recipient != null))
								continue;

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

								new AdminInterface.Helpers.Printer().Print(brail, invoice);
							}
						}
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
					ProcessStarter.StartProcessInteractivly(exe + " print " + arguments, "KvasovSam", "vbrhjcrjgbxtcrbq", "analit");
				}
			}
			catch (Exception e)
			{
				logger.Error("ошибка", e);
				
			}
		}
	}
}
