using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Helper(typeof(BindingHelper))]
	public class InvoicesController : AdminInterfaceController
	{
		public void Index([DataBind("filter")] PayerDocumentFilter filter)
		{
			if (filter.Region != null && filter.Region.Id == 0)
				filter.Region = null;
			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;

			PropertyBag["filter"] = filter;
			PropertyBag["invoices"] = filter.Find<Invoice>();
			PropertyBag["buildFilter"] = new DocumentBuilderFilter();

			PropertyBag["printers"] = Printer.All();
		}

		public void Process()
		{
			var binder = new ARDataBinder();
			binder.AutoLoad = AutoLoadBehavior.Always;
			SetBinder(binder);
			var invoices = BindObject<Invoice[]>("invoices");

			if (Form["delete"] != null)
			{
				foreach (var act in invoices)
					act.Delete();

				Notify("Удалено");
				RedirectToReferrer();
			}
			if (Form["print"] != null)
			{
				var printer = Form["printer"];
				var arguments = String.Format("invoice \"{0}\" \"{1}\"", printer, invoices.Implode(i => i.Id));
				Printer.Execute(arguments);

				Notify("Отправлено на печать");
				RedirectToReferrer();
			}
			if (Form["mail"] != null)
			{
				foreach (var invoice in invoices)
					this.Mailer().Invoice(invoice, true).Send();

				Notify("Отправлено");
				RedirectToReferrer();
			}
		}

		public void PrintIndex([DataBind("filter")] PayerDocumentFilter filter)
		{
			LayoutName = "Print";
			if (filter.Region != null && filter.Region.Id == 0)
				filter.Region = null;
			else if (filter.Region != null)
				filter.Region = Region.Find(filter.Region.Id);

			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;
			else if (filter.Recipient != null)
				filter.Recipient = Recipient.Find(filter.Recipient.Id);

			PropertyBag["filter"] = filter;
			PropertyBag["invoices"] = filter.Find<Invoice>();
		}

		public void Cancel(uint id)
		{
			var invoice = Invoice.Find(id);
			invoice.Delete();
			Notify("Сохранено");
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
			RenderView("/Payers/NewInvoice");

			var invoice = Invoice.Find(id);
			PropertyBag["invoice"] = invoice;
			PropertyBag["references"] = Nomenclature.Queryable.OrderBy(n => n.Name).ToList();

			if (IsPost)
			{
				DoNotRecreateCollectionBinder.Prepare(this, "invoice.Parts");

				BindObjectInstance(invoice, "invoice");
				if (!HasValidationError(invoice))
				{
					invoice.CalculateSum();
					invoice.Save();
					Notify("Сохранено");
					Redirect("Invoices", "Edit", new {invoice.Id});
				}
			}
		}

		public void Build([ARDataBind("buildFilter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] DocumentBuilderFilter filter, DateTime invoiceDate, string printer)
		{
			var invoices = BuildInvoices(filter, invoiceDate);

			invoices = invoices.Where(i => i.Payer.InvoiceSettings.PrintInvoice).ToList();
			if (invoices.Count > 0)
			{
				var arguments = String.Format("invoice \"{0}\" \"{1}\"", printer, invoices.Implode(i => i.Id));
				Printer.Execute(arguments);
			}

			Notify("Счета сформированы");
			RedirectToAction("Index", filter.ToDocumentFilter().GetQueryString());
		}

		private static List<Invoice> BuildInvoices(DocumentBuilderFilter filter, DateTime invoiceDate)
		{
			var invoicePeriod = filter.Period.GetInvoicePeriod();
			IList<Payer> payers = null;
			if (filter.PayerId == null || filter.PayerId.Length == 0)
			{
				ArHelper.WithSession(s => {
					var query = s.QueryOver<Payer>()
						.Where(p => p.AutoInvoice == InvoiceType.Auto
							&& p.PayCycle == invoicePeriod
							&& p.Recipient != null);

					if (filter.Recipient != null)
						query.Where(p => p.Recipient == filter.Recipient);

					query.OrderBy(p => p.Name);
					payers = query.List<Payer>();
				});

				if (filter.Region != null)
					payers = payers.Where(p => p.Clients.Any(c => c.HomeRegion == filter.Region)).ToList();
			}
			else
			{
				payers = ActiveRecordLinqBase<Payer>.Queryable
					.Where(p => filter.PayerId.Contains(p.PayerID))
					.OrderBy(p => p.Name)
					.ToList();
			}

			var invoices = new List<Invoice>();
			foreach (var payer in payers)
			{
				if (Invoice.Queryable.Any(i => i.Payer == payer && i.Period == filter.Period))
					continue;

				foreach (var invoice in payer.BuildInvoices(invoiceDate, filter.Period).Where(i => i.Sum > 0))
				{
					invoices.Add(invoice);
					invoice.Save();
				}
			}
			return invoices;
		}
	}
}