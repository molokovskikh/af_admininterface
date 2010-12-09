using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord.Linq;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Controllers
{
	public class PaymentFilter
	{
		public Recipient Recipient { get; set; }
		public DatePeriod Period { get; set; }
		public string SearchText { get; set; }

		public PaymentFilter()
		{
			Period = new DatePeriod {
				Begin = DateTime.Today,
				End = DateTime.Today
			};
		}

		public List<Recipient> Recipients
		{
			get { return Recipient.Queryable.OrderBy(r => r.Name).ToList(); }
		}

		public List<Payment> Find()
		{
			var criteria = DetachedCriteria.For<Payment>()
				.Add(Expression.Ge("PayedOn", Period.Begin) && Expression.Le("PayedOn", Period.End));

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			return ArHelper.WithSession(s =>
				criteria.GetExecutableCriteria(s)
					.List<Payment>()).ToList();
		}
	}

	[Layout("GeneralWithJQueryOnly")]
	public class PaymentsController : ARSmartDispatcherController
	{
		public void Index([DataBind("filter")] PaymentFilter filter)
		{
			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;

			PropertyBag["filter"] = filter;
			PropertyBag["payments"] = filter.Find();
		}

		public void New()
		{
			if (IsPost)
			{
				var payment = new Payment();
				BindObjectInstance(payment, "payment", AutoLoadBehavior.OnlyNested);
				payment.RegisterPayment();
				payment.Save();
				RedirectToReferrer();
			}
			else
			{
				PropertyBag["recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();
				PropertyBag["payments"] = Payment.Queryable
					.Where(p => p.RegistredOn >= DateTime.Today)
					.OrderBy(p => p.RegistredOn).ToList();
			}
		}

		[return: JSONReturnBinder]
		public object SearchPayer(string term)
		{
			return ActiveRecordLinq
				.AsQueryable<Payer>()
				.Where(p => p.ShortName.Contains(term))
				.Take(20)
				.Select(p => new {
					id = p.PayerID,
					label = p.ShortName
				});
		}
	}
}