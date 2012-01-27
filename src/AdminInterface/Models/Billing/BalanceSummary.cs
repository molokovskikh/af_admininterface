using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Billing
{
	public class BalanceSummary
	{
		public BalanceSummary(DateTime begin, DateTime end, Payer payer)
		{
			var invoices = Invoice.Queryable.Where(p => p.Payer == payer).ToList();
			var payments = Payment.Queryable.Where(p => p.Payer == payer).ToList();
			var acts = Act.Queryable.Where(p => p.Payer == payer).ToList();
			var operations = ActiveRecordLinqBase<BalanceOperation>.Queryable.Where(d => d.Payer == payer).ToList();
			var refunds = operations.Where(d => d.Type == OperationType.Refund);
			var reliefs = operations.Where(d => d.Type == OperationType.DebtRelief);
			var items = invoices
				.Select(i => new { i.Id, i.Date, i.Sum, IsInvoice = true, IsAct = false, IsPayment = false, IsOperation = false, Object  = (object)i })
				.Union(payments.Select(p => new { p.Id, Date = p.PayedOn, p.Sum, IsInvoice = false, IsAct = false, IsPayment = true, IsOperation = false, Object = (object)p }))
				.Union(acts.Select(a => new { a.Id, Date = a.ActDate, a.Sum, IsInvoice = false, IsAct = true, IsPayment = false, IsOperation = false, Object = (object)a }))
				.Union(refunds.Select(d => new { d.Id, d.Date, Sum = Decimal.Negate(d.Sum), IsInvoice = true, IsAct = false, IsPayment = false, IsOperation = true, Object = (object)d }))
				.Union(reliefs.Select(d => new { d.Id, d.Date, Sum = Decimal.Negate(d.Sum), IsInvoice = false, IsAct = true, IsPayment = false, IsOperation = true, Object = (object)d }))
				.ToList();

			var befores = items.Where(i => i.Date < begin);
			Before = befores
				.Select(i => i.Object)
				.OfType<IBalanceUpdater>()
				.Sum(i => i.BalanceAmount);
			if (payer.BeginBalanceDate.HasValue)
				Before += payer.BeginBalance;

			items = items.Where(i => i.Date >= begin && i.Date <= end).ToList();
			Total = items.Select(i => i.Object).OfType<IBalanceUpdater>().Sum(i => i.BalanceAmount);
			Total += Before;

			Items = items.OrderBy(i => i.Date).ToList();
		}

		public decimal Before { get; private set; }
		public decimal Total { get; private set; }
		public IEnumerable<object> Items { get; set; }
	}
}