using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public class PayerDocumentFilter
	{
		public Period? Period { get; set; }
		public Region Region { get; set; }
		public Recipient Recipient { get; set; }
		public string SearchText { get; set; }

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			if (Period != null)
				criteria.Add(Expression.Eq("Period", Period));

			if (Region != null)
				criteria.CreateCriteria("p.Clients", "c")
					.Add(Expression.Eq("c.HomeRegion", Region));

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("p.Name", SearchText, MatchMode.Anywhere));

			return ArHelper.WithSession(s => criteria
				.GetExecutableCriteria(s).List<T>())
				.ToList()
				.GroupBy(i => ((dynamic)i).Id)
				.Select(g => g.First())
				.ToList();
		}

		public string[] ToUrl()
		{
			return new [] {
				String.Format("filter.Period={0}", (int)Period),
				String.Format("filter.Region.Id={0}", Region.Id),
				String.Format("filter.Recipient.Id={0}", Recipient.Id),
			};
		}
	}

	public class DocumentBuilderFilter
	{
		public Period Period { get; set; }
		public Region Region { get; set; }
		public Recipient Recipient { get; set; }

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			criteria.Add(Expression.Eq("Period", Period));

			if (Region != null)
				criteria.CreateCriteria("p.Clients", "c")
					.Add(Expression.Eq("c.HomeRegion", Region));

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			List<T> items = null;

			ArHelper.WithSession(s => {
				items = criteria
				.GetExecutableCriteria(s).List<T>()
				.GroupBy(i => ((dynamic)i).Id)
				.Select(g => g.First())
				.ToList();
			});
			return items;
		}

		public Dictionary<string, string> ToUrl()
		{
			var map = new Dictionary<string, string> {
				{"filter.Period", Period.ToString()},
			};
			if (Region != null)
				map.Add("filter.Region.Id", Region.Id.ToString());

			if (Recipient != null)
				map.Add("filter.Recipient.Id", Recipient.Id.ToString());
			return map;
		}
	}

	[
		Layout("GeneralWithJQueryOnly"),
		Helper(typeof(BindingHelper))
	]
	public class InvoicesController : SmartDispatcherController
	{
		public void Index([DataBind("filter")] PayerDocumentFilter filter)
		{
			if (filter.Region != null && filter.Region.Id == 0)
				filter.Region = null;
			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;

			PropertyBag["filter"] = filter;
			PropertyBag["invoices"] = filter.Find<Invoice>();

			PropertyBag["printers"] = PrinterSettings.InstalledPrinters
				.Cast<string>()
#if !DEBUG
				.Where(p => p.Contains("Бух"))
				.OrderBy(p => p)
#endif
				.ToList();
			PropertyBag["regions"] = Region.Queryable.OrderBy(r => r.Name).ToList();
			PropertyBag["recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();
		}

		public void Cancel(uint id)
		{
			var invoice = Invoice.Find(id);
			invoice.Delete();
			Flash["Message"] = "Сохранено";
			RedirectToReferrer();
		}

		public void Print(uint id)
		{
			LayoutName = "Print";
			PropertyBag["invoice"] = Invoice.Find(id);
			PropertyBag["doc"] = PropertyBag["invoice"];
		}

		public void Edit(uint id)
		{
			Binder.Validator = Validator;
			RenderView("/Payers/NewInvoice");

			var invoice = Invoice.Find(id);
			PropertyBag["invoice"] = invoice;

			if (IsPost)
			{
				BindObjectInstance(invoice, "invoice");
				if (!HasValidationError(invoice))
				{
					invoice.CalculateSum();
					invoice.Save();
					Flash["Message"] = "Сохранено";
					Redirect("Invoices", "Edit", new {invoice.Id});
				}
			}
		}

		public void Build(Period period, ulong regionId, string printer, DateTime invoiceDate, uint recipientId)
		{
/*			var invoicePeriod = InvoicePeriod.Month;
			if (period == Period.FirstQuarter ||
				period == Period.SecondQuarter ||
				period == Period.FirstQuarter ||
				period == Period.FourthQuarter)
				invoicePeriod = InvoicePeriod.Quarter;*/

/*
			var payers = ActiveRecordLinqBase<Payer>
				.Queryable
				.Where(p => p.AutoInvoice == InvoiceType.Auto && p.PayCycle == invoicePeriod);
*/

			//Printer.SetPrinter(printer);

/*
			foreach (var payer in payers)
			{
				if (!payer.JuridicalOrganizations.Any(j => j.Recipient != null))
					continue;

				if (Invoice.Queryable.Any(i => i.Payer == payer && i.Period == period))
					continue;

				var invoice = new Invoice(payer, period);
				invoice.Send(this);
				invoice.Save();
			}
*/

			var printerPath = @"U:\Apps\Printer\Printer.exe";
#if DEBUG
			printerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Printer\bin\debug\Printer.exe");
#endif
			var info = new System.Diagnostics.ProcessStartInfo(printerPath,
				String.Format("invoice \"{0}\" {1} {2} {3} {4}", printer, period, regionId, invoiceDate.ToShortDateString(), recipientId));
			var process = System.Diagnostics.Process.Start(info);
			process.WaitForExit(30*1000);

			Flash["message"] = String.Format("Счета за {0} сформированы", BindingHelper.GetDescription(period));
			RedirectToAction("Index",
				new PayerDocumentFilter {
					Period = period,
					Region = Region.Find(regionId),
					Recipient = Recipient.Find(recipientId)
				}.ToUrl());
		}
	}
}