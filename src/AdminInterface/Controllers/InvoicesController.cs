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
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(PaginatorHelper), "paginator"),
	]
	public class InvoicesController : AdminInterfaceController
	{
		public void Index([SmartBinder] PayerDocumentFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["invoices"] = filter.Find<Invoice>();
			PropertyBag["buildFilter"] = new DocumentBuilderFilter();

			PropertyBag["printers"] = Printer.All();
		}

		public void Process()
		{
			var filter = (PayerDocumentFilter)BindObject(ParamStore.Form, typeof(PayerDocumentFilter), "filter");
			var binder = new ARDataBinder();
			binder.AutoLoad = AutoLoadBehavior.Always;
			SetBinder(binder);
			var invoices = BindObject<Invoice[]>("invoices");

			if (Form["delete"] != null) {
				foreach (var act in invoices)
					DbSession.Delete(act);

				Notify("Удалено");
				RedirectToReferrer();
			}
			if (Form["print"] != null) {
				var printer = Form["printer"];
				filter.PrepareFindActInvoiceIds(invoices.Implode(i => i.Id));
				var arguments = String.Format("invoice \"{0}\" \"{1}\"", printer, filter.Find<Invoice>().Implode(i => i.Id));
				Printer.Execute(arguments);

				Notify("Отправлено на печать");
				RedirectToReferrer();
			}
			if (Form["mail"] != null) {
				foreach (var invoice in invoices)
					this.Mailer().InvoiceToEmail(invoice, true).Send();

				Notify("Отправлено");
				RedirectToReferrer();
			}
		}

		public void PrintIndex([SmartBinder] PayerDocumentFilter filter)
		{
			LayoutName = "Print";
			PropertyBag["filter"] = filter;
			PropertyBag["invoices"] = filter.Find<Invoice>();
		}

		public void Cancel(uint id)
		{
			var invoice = DbSession.Load<Invoice>(id);
			DbSession.Delete(invoice);
			Notify("Сохранено");
			RedirectToReferrer();
		}

		public void Print(uint id)
		{
			LayoutName = "Print";
			PropertyBag["invoice"] = DbSession.Load<Invoice>(id);
			PropertyBag["doc"] = PropertyBag["invoice"];
		}

		public void Edit(uint id)
		{
			RenderView("/Payers/NewInvoice");

			var invoice = DbSession.Load<Invoice>(id);
			PropertyBag["invoice"] = invoice;
			PropertyBag["references"] = DbSession.Query<Nomenclature>().OrderBy(n => n.Name).ToList();

			if (IsPost) {
				RecreateOnlyIfNullBinder.Prepare(this, "invoice.Parts");

				BindObjectInstance(invoice, "invoice");
				if (!HasValidationError(invoice)) {
					invoice.CalculateSum();
					DbSession.Save(invoice);
					Notify("Сохранено");
					Redirect("Invoices", "Edit", new { invoice.Id });
				}
			}
		}

		public void Build([ARDataBind("buildFilter", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] DocumentBuilderFilter filter, DateTime invoiceDate, string printer)
		{
			var createdTime = DateTime.Now;
			filter.BuildInvoices(invoiceDate);

			Notify("Счета сформированы");
			var destinationFilter = filter.ToDocumentFilter();
			destinationFilter.CreatedOn = createdTime;
			RedirectToAction("Index", destinationFilter.GetQueryString());
		}
	}
}