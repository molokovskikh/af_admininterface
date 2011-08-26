using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Controller=AdminInterface.MonoRailExtentions.Controller;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
	]
	public class PayersController : Controller
	{
		public void Show(uint id)
		{
			var request = Request.QueryString.AllKeys.ToDictionary(k => k, k => Request.QueryString[k]);
			request.Remove("id");
			request.Add("BillingCode", id.ToString());
			Redirect("Billing", "Edit", request);
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
				.ToList();
			if (payer.BeginBalance != 0 && payer.BeginBalanceDate.HasValue)
				items.Add(new {Id = 0u, Date = payer.BeginBalanceDate.Value, Sum = payer.BeginBalance, IsInvoice = false, IsAct = false, IsPayment = true});

			items = items.OrderBy(i => i.Date).ToList();

			PropertyBag["payer"] = payer;
			PropertyBag["items"] = items;
		}

		public void NewPayment(uint id)
		{
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

		public void Ad(uint id)
		{
			var payer = Payer.Find(id);
			PropertyBag["payer"] = payer;
			PropertyBag["ads"] = Advertising.Queryable.Where(a => a.Payer == payer)
				.OrderBy(a => a.Begin).ToList();
		}

		public void NewAd(uint id)
		{
			var payer = Payer.Find(id);
			var ad = new Advertising(payer);
			if (IsPost)
			{
				BindObjectInstance(ad, "ad");
				if (!HasValidationError(ad))
				{
					ad.Save();
					Redirect("Payers", "Ad", new{payer.Id});
				}
			}
			PropertyBag["ad"] = ad;
			PropertyBag["payer"] = payer;
		}
	}
}