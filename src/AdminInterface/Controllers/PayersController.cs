using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
	]
	public class PayersController : AdminInterfaceController
	{
		public void Show(uint id)
		{
			var request = Request.QueryString.AllKeys.ToDictionary(k => k, k => Request.QueryString[k]);
			request.Remove("id");
			request.Add("BillingCode", id.ToString());
			Redirect("Billing", "Edit", request);
		}

		public void BalanceSummary(uint id, int year)
		{
			CancelLayout();

			var begin = new DateTime(year, 1, 1);
			var end = new DateTime(year, 12, 31);

			var payer = Payer.Find(id);
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
			var before = befores
				.Select(i => i.Object)
				.OfType<IBalanceUpdater>()
				.Sum(i => i.BalanceAmount);
			if (payer.BeginBalanceDate.HasValue)
				before += payer.BeginBalance;

			items = items.Where(i => i.Date >= begin && i.Date <= end).ToList();
			var total = items.Select(i => i.Object).OfType<IBalanceUpdater>().Sum(i => i.BalanceAmount);

			items = items.OrderBy(i => i.Date).ToList();

			PropertyBag["year"] = year;
			PropertyBag["operation"] = new BalanceOperation(payer);
			PropertyBag["payer"] = payer;
			PropertyBag["items"] = items;

			if (before != 0)
				PropertyBag["before"] = before;

			if (total != 0)
				PropertyBag["total"] = total;
		}

		public void NewPayment(uint id, int year)
		{
			var payer = Payer.Find(id);
			var payment = new Payment(payer);
			BindObjectInstance(payment, "payment");
			if (!HasValidationError(payment))
			{
				Notify("Сохранено");
				payment.RegisterPayment();
				payment.Save();
			}
			else
			{
				Error(GetFirstErrorWithProperty(payment));
			}

			//мы должны возвращаться на туже вкладку с которой был совершен переход
			//RedirectToReferrer();
			RedirectToUrl(String.Format("~/Billing/Edit?BillingCode={0}#tab-balance-summary-{1}", payer.Id, year));
		}

		public void NewBalanceOperation(uint id, int year)
		{
			var payer = Payer.Find(id);
			var operation = new BalanceOperation(payer);
			BindObjectInstance(operation, "operation");
			if (IsValid(operation))
			{
				ActiveRecordMediator.Save(operation);
				Notify("Сохранено");
			}
			else
			{
				Error(GetFirstErrorWithProperty(operation));
			}

			//мы должны возвращаться на туже вкладку с которой был совершен переход
			RedirectToUrl(String.Format("~/Billing/Edit?BillingCode={0}#tab-balance-summary-{1}", payer.Id, year));
		}

		private string GetFirstErrorWithProperty(object item)
		{
			var property = Validator.GetErrorSummary(item).InvalidProperties.First();
			var message = Validator.GetErrorSummary(item).GetErrorsForProperty(property).First();
			var description = BindingHelper.TryGetDescription(item.GetType(), property);

			if (String.IsNullOrEmpty(description))
				return message;
			else
				return String.Format("{0} - {1}", description, message);
		}

		public void InvoicePreview(uint id, int group)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Error("У плательщика не указан получатель платежей, выберете получателя платежей.");
				RedirectToReferrer();
				return;
			}

			var invoice = new Invoice(payer, DateTime.Now.ToPeriod(), DateTime.Now, group);
			PropertyBag["doc"] = invoice;
			PropertyBag["invoice"] = invoice;
			LayoutName = "Print";
			RenderView("/Invoices/Print");
		}

		public void NewInvoice(uint id)
		{
			var payer = Payer.Find(id);
			var invoice = new Invoice(payer);
			PropertyBag["invoice"] = invoice;
			PropertyBag["references"] = Nomenclature.Queryable.OrderBy(n => n.Name).ToList();

			if (IsPost)
			{
				BindObjectInstance(invoice, "invoice");
				if (!HasValidationError(invoice))
				{
					invoice.SetPayer(payer);
					invoice.CalculateSum();
					invoice.Save();
					Notify("Счет сформирован");
					Redirect("Billing", "Edit", new {BillingCode = payer.Id});
				}
			}
			else
			{
				invoice.Parts.Add(new InvoicePart(invoice));
			}
		}

		public void NewAct(uint id)
		{
			var payer = Payer.Find(id);
			var act = new Act(payer, DateTime.Now);
			PropertyBag["act"] = act;
			PropertyBag["references"] = Nomenclature.Queryable.OrderBy(n => n.Name).ToList();

			if (IsPost)
			{
				BindObjectInstance(act, "act");
				if (IsValid(act))
				{
					act.SetPayer(payer);
					act.CalculateSum();
					act.Save();
					Notify("Акт сформирован");
					Redirect("Billing", "Edit", new {BillingCode = payer.Id});
				}
			}
			else
			{
				act.Parts.Add(new ActPart(act));
			}
			RenderView("/Acts/Edit");
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

		public void InvoiceGroups(uint id)
		{
			var payer = Payer.Find(id);

			if (payer.GetAccounts().Count() == 0)
				Error("Нет ни одной позиции для формирования счета");

			if (IsPost)
			{
				SetBinder(new AccountBinder());
				((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.Always;
				var accounts = BindObject<Account[]>(ParamStore.Form, "accounts");
				foreach (var account in accounts)
					account.Save();

				Notify("Сохранено");
				RedirectToReferrer();
			}

			PropertyBag["payer"] = payer;
		}
	}
}