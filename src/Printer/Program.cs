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

					var invoicePeriod = InvoicePeriod.Month;
					if (period == Period.FirstQuarter ||
						period == Period.SecondQuarter ||
						period == Period.FirstQuarter ||
						period == Period.FourthQuarter)
						invoicePeriod = InvoicePeriod.Quarter;

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

							var invoice = new Invoice(payer, period);
							if (payer.InvoiceSettings.PrintInvoice)
							{
								new AdminInterface.Helpers.Printer().Print(brail, invoice);
								payer.Balance -= invoice.Sum;
							}

							using (var scope = new TransactionScope(OnDispose.Rollback))
							{
								invoice.Save();
								scope.VoteCommit();
							}
						}
					}
				}
				else
				{
					var assembly = Assembly.GetExecutingAssembly();
					var exe = assembly.Location;
					ProcessStarter.StartProcessInteractivly(exe + " print " + String.Join(" ", args), "KvasovSam", "vbrhjcrjgbxtcrbq", "analit");
				}
			}
			catch (Exception e)
			{
				logger.Error("ошибка", e);
				
			}
		}
	}
}
