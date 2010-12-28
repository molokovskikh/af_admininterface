using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[Helper(typeof(BindingHelper))]
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
	}
}