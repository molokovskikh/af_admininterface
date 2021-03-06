﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Linq;
using NHibernate.SqlCommand;

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
			var payer = DbSession.Load<Payer>(id);
			var summary = new BalanceSummary(begin, end, payer);

			PropertyBag["year"] = year;
			PropertyBag["operation"] = new BalanceOperation(payer);
			PropertyBag["payer"] = payer;
			PropertyBag["items"] = summary.Items;
			PropertyBag["summary"] = summary;
			PropertyBag["before"] = summary.Before;
			PropertyBag["total"] = summary.Total;
		}

		public void NewPayment(uint id, int year)
		{
			var payer = DbSession.Load<Payer>(id);
			var payment = new Payment(payer);
			BindObjectInstance(payment, "payment");
			if (IsValid(payment)) {
				Notify("Сохранено");
				payment.RegisterPayment();
				DbSession.Save(payment);
			}
			else {
				Error(GetFirstErrorWithProperty(payment));
			}

			//мы должны возвращаться на туже вкладку с которой был совершен переход
			//RedirectToReferrer();
			RedirectToUrl(String.Format("~/Billing/Edit?BillingCode={0}#tab-balance-summary-{1}", payer.Id, year));
		}

		public void NewBalanceOperation(uint id, int year)
		{
			var payer = DbSession.Load<Payer>(id);
			var operation = new BalanceOperation(payer);
			BindObjectInstance(operation, "operation");
			if (IsValid(operation)) {
				ActiveRecordMediator.Save(operation);
				Notify("Сохранено");
			}
			else {
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
			var payer = DbSession.Load<Payer>(id);
			if (payer.Recipient == null) {
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
			var payer = DbSession.Load<Payer>(id);
			var invoice = new Invoice(payer);
			PropertyBag["invoice"] = invoice;
			PropertyBag["references"] = DbSession.Query<Nomenclature>().OrderBy(n => n.Name).ToList();

			if (IsPost) {
				BindObjectInstance(invoice, "invoice");
				if (IsValid(invoice)) {
					invoice.SetPayer(payer);
					invoice.CalculateSum();
					DbSession.Save(invoice);
					Notify("Счет сформирован");
					Redirect("Billing", "Edit", new { BillingCode = payer.Id });
				}
			}
			else {
				invoice.Parts.Add(new InvoicePart(invoice));
			}
		}

		public void NewAct(uint id)
		{
			var payer = DbSession.Load<Payer>(id);
			var act = new Act(payer, DateTime.Now);
			PropertyBag["act"] = act;
			PropertyBag["references"] = DbSession.Query<Nomenclature>().OrderBy(n => n.Name).ToList();

			if (IsPost) {
				BindObjectInstance(act, "act");
				if (IsValid(act)) {
					act.SetPayer(payer);
					act.CalculateSum();
					DbSession.Save(act);
					Notify("Акт сформирован");
					Redirect("Billing", "Edit", new { BillingCode = payer.Id });
				}
			}
			else {
				act.Parts.Add(new ActPart(act));
			}
			RenderView("/Acts/Edit");
		}

		public void Ad(uint id)
		{
			var payer = DbSession.Load<Payer>(id);
			PropertyBag["payer"] = payer;
			PropertyBag["ads"] = DbSession.Query<Advertising>()
				.Where(a => a.Payer == payer)
				.OrderBy(a => a.Begin)
				.ToList();
		}

		public void NewAd(uint id)
		{
			var payer = DbSession.Load<Payer>(id);
			var ad = new Advertising(payer);
			if (IsPost) {
				BindObjectInstance(ad, "ad");
				if (IsValid(ad)) {
					DbSession.Save(ad);
					Redirect("Payers", "Ad", new { payer.Id });
				}
			}
			PropertyBag["ad"] = ad;
			PropertyBag["payer"] = payer;
		}

		public void InvoiceGroups(uint id)
		{
			var payer = DbSession.Load<Payer>(id);

			if (payer.GetAccounts().Count() == 0)
				Error("Нет ни одной позиции для формирования счета");

			if (IsPost) {
				SetBinder(new AccountBinder());
				((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.Always;
				var accounts = BindObject<Account[]>(ParamStore.Form, "accounts");
				foreach (var account in accounts)
					DbSession.Save(account);

				Notify("Сохранено");
				RedirectToReferrer();
			}

			PropertyBag["payer"] = payer;
		}

		public void Delete(uint id, string deleteComment)
		{
			var payer = DbSession.Load<Payer>(id);

			try {
				var notifyPayer = payer;
				payer.Delete(DbSession);
				Mail().PayerDelete(notifyPayer, deleteComment);
				Notify("Удалено");
				Redirect("Billing", "Search");
			}
			catch (EndUserException e) {
				Error(e.Message);
				RedirectToReferrer();
			}
		}

		public void Messages(uint id)
		{
			var payer = DbSession.Load<Payer>(id);

			var filter = new PayerMessagesFilter {
				Payer = payer,
				DbSession = DbSession
			};

			BindObjectInstance(filter, "filter");

			PropertyBag["payer"] = payer;
			PropertyBag["filter"] = filter;
			PropertyBag["messages"] = filter.Find();
		}
	}

	public class PayerMessagesFilter
	{
		public ISession DbSession { get; set; }

		public Payer Payer { get; set; }
		public DatePeriod Period { get; set; }

		public PayerMessagesFilter()
		{
			Period = new DatePeriod(DateTime.Today.AddDays(-14), DateTime.Today);
		}

		public IList<UserMessageSendLog> Find()
		{
			return DbSession.QueryOver<UserMessageSendLog>()
				.Where(m => m.LogTime >= Period.Begin && m.LogTime < Period.End.AddDays(1))
				.OrderBy(m => m.LogTime).Desc
				.Inner.JoinQueryOver(m => m.User)
				.Where(u => u.Payer == Payer)
				.List();
		}
	}
}