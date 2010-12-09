using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord.Linq;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public class InvoiceFilter
	{
		public Period? Period { get; set; }
		public Region Region { get; set; }
		public Recipient Recipient { get; set; }
		public string SearchText { get; set; }

		public IList<Region> Regions
		{
			get
			{
				return ActiveRecordLinqBase<Region>.Queryable
					.Where(r => r.Id != 0)
					.OrderBy(r => r.Name)
					.ToList();
			}
		}

		public IList<Recipient> Recipients
		{
			get
			{
				return Recipient.Queryable.OrderBy(r => r.Name).ToList();
			}
		}

		public List<Invoice> Find()
		{
			var criteria = DetachedCriteria.For<Invoice>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			if (Period != null)
				criteria.Add(Expression.Eq("Period", Period));

			if (Region != null)
			{
				criteria.CreateCriteria("p.Clients", "c")
					.Add(Expression.Eq("c.HomeRegion", Region));
			}

			if (Recipient != null)
			{
				criteria.Add(Expression.Eq("Recipient", Recipient));
			}

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("p.ShortName", SearchText, MatchMode.Anywhere));

			return ArHelper.WithSession(s => criteria
				.GetExecutableCriteria(s).List<Invoice>())
				.ToList();
		}
	}

	[
		Layout("GeneralWithJQueryOnly"),
		Helper(typeof(BindingHelper))
	]
	public class InvoicesController : SmartDispatcherController
	{
		public void Index([DataBind("filter")] InvoiceFilter filter)
		{
			if (filter.Region != null && filter.Region.Id == 0)
				filter.Region = null;
			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;

			PropertyBag["filter"] = filter;
			PropertyBag["printers"] = PrinterSettings.InstalledPrinters;
			PropertyBag["invoices"] = filter.Find();
		}

		public void Print(uint id)
		{
			LayoutName = "Print";
			PropertyBag["invoice"] = Invoice.Find(id);
		}

		public void Build(Period period, string printer)
		{
			var invoicePeriod = InvoicePeriod.Month;
			if (period == Period.FirstQuarter ||
				period == Period.SecondQuarter ||
				period == Period.FirstQuarter ||
				period == Period.FourthQuarter)
				invoicePeriod = InvoicePeriod.Quarter;

			var payers = ActiveRecordLinqBase<Payer>
				.Queryable
				.Where(p => p.AutoInvoice == InvoiceType.Auto && p.PayCycle == invoicePeriod);

			Printer.SetPrinter(printer);

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

			Flash["message"] = String.Format("����� �� {0} ������������", BindingHelper.GetDescription(period));
			RedirectToAction("Index");
		}
	}
}