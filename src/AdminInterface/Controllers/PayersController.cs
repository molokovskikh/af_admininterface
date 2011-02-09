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
		public void Show(uint id)
		{
			Redirect("Billing", "Edit", new {BillingCode = id});
		}

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
			var acts = Act.Queryable.Where(p => p.Payer == payer).ToList();
			var items = invoices
				.Select(i => new { i.Id, i.Date, i.Sum, IsInvoice = true, IsAct = false, IsPayment = false })
				.Union(payments.Select(p => new {p.Id, Date = p.PayedOn, p.Sum, IsInvoice = false, IsAct = false, IsPayment = true}))
				.Union(acts.Select(a => new {a.Id, Date = a.ActDate, a.Sum, IsInvoice = false, IsAct = true, IsPayment = false}))
				.OrderByDescending(i => i.Date)
				.ToList();
			PropertyBag["payer"] = payer;
			PropertyBag["items"] = items;
		}

		public void NewPayment(uint id)
		{
			Binder.Validator = Validator;
			var payer = Payer.Find(id);
			var payment = new Payment(payer);
			BindObjectInstance(payment, "payment");
			if (!HasValidationError(payment))
			{
				payment.RegisterPayment();
				payment.Save();
			}
			RedirectToReferrer();
		}

		public void NewInvoice(uint id)
		{
			Binder.Validator = Validator;
			var payer = Payer.Find(id);
			var invoice = new Invoice(payer);
			PropertyBag["invoice"] = invoice;

			if (IsPost)
			{
				BindObjectInstance(invoice, "invoice");
				if (!HasValidationError(invoice))
				{
					invoice.SetPayer(payer);
					invoice.Sum = invoice.Parts.Sum(p => p.Sum);
					invoice.Save();
					Redirect("Billing", "Edit", new {BillingCode = payer.Id});
				}
			}
			else
			{
				invoice.Parts.Add(new InvoicePart());
			}
		}
	}
}