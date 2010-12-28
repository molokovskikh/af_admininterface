using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Layout("GeneralWithJQueryOnly")
	]
	public class PayersController : SmartDispatcherController
	{
		public void Payments(uint id)
		{
			var payer = Payer.Find(id);
			PropertyBag["payments"] = Payment.Queryable.Where(p => p.Payer == payer).ToList();
		}

		public void Invoices(uint id)
		{
			var payer = Payer.Find(id);
			PropertyBag["invoices"] = Invoice.Queryable.Where(p => p.Payer == payer).ToList();
		}

		public void BalanceSummary(uint id)
		{
			CancelLayout();

			var payer = Payer.Find(id);
			var invoices = Invoice.Queryable.Where(p => p.Payer == payer).ToList();
			var payments = Payment.Queryable.Where(p => p.Payer == payer).ToList();
			var items = invoices
				.Select(i => new { i.Date, i.Sum, IsInvoice = true, IsAct = false, IsPayment = false })
				.Union(payments.Select(p => new {Date = p.PayedOn, p.Sum, IsInvoice = false, IsAct = false, IsPayment = true}))
				.OrderByDescending(i => i.Date)
				.ToList();
			PropertyBag["items"] = items;
		}

		public void NewInvoice(uint id)
		{
			var payer = Payer.Find(id);
			if (IsPost)
			{
				var invoice = BindObject<Invoice>("invoice");
				invoice.SetPayer(payer);
				invoice.Sum = invoice.Parts.Sum(p => p.Sum);
				invoice.Save();
				Redirect("Billing", "Edit", new {BillingCode = payer.Id});
			}
		}
	}
}