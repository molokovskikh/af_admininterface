using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;
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
			var arguments = String.Format("invoice \"{0}\" {1} {2} {3} {4}", printer, period, regionId, invoiceDate.ToShortDateString(), recipientId);
			Printer.Execute(arguments);

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