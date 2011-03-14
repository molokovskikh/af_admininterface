using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
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
		public bool ShowOnlyUnknown { get; set; }

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
				.Add(Expression.Ge("PayedOn", Period.Begin) && Expression.Le("PayedOn", Period.End.AddDays(1)));

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			if (ShowOnlyUnknown)
				criteria.Add(Expression.IsNull("Payer"));

			if (!String.IsNullOrWhiteSpace(SearchText))
			{
				criteria.CreateAlias("Payer", "p");
				criteria.Add(Expression.Like("p.Name", SearchText));
			}

			return ArHelper.WithSession(s =>
				criteria.GetExecutableCriteria(s)
					.List<Payment>()).ToList();
		}
	}

	public class PaymentStatistics
	{
		public PaymentStatistics(List<Payment> payments)
		{
			Count = payments.Count;
			Sum = payments.Sum(p => p.Sum);
		}

		public int Count { get; set; }
		public decimal Sum { get; set; }
	}


	[Layout("GeneralWithJQueryOnly")]
	public class PaymentsController : ARSmartDispatcherController
	{
		public void Index([DataBind("filter")] PaymentFilter filter)
		{
			if (filter.Recipient != null && filter.Recipient.Id == 0)
				filter.Recipient = null;

			var payments = filter.Find();
			PropertyBag["filter"] = filter;
			PropertyBag["payments"] = payments;
			PropertyBag["stat"] = new PaymentStatistics(payments);
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

		public void SavePayments()
		{
			var payments = TempPayments();
			if (payments == null)
			{
				Flash["Message"] = Message.Error("Время сесии истекло. Загрузите выписку повторно.");
				RedirectToReferrer();
			}

			foreach (var payment in payments)
			{
				//если зайти в два платежа и отредактировать их
				//то получим двух плательщиков из разных сесей
				//правим это
				if (payment.Payer != null)
					payment.Payer = Payer.Find(payment.Payer.Id);

				payment.RegisterPayment();
				payment.Save();
			}

			RedirectToAction("Index",
				new Dictionary<string, string>{
					{"filter.Period.Begin", payments.Min(p => p.PayedOn).ToShortDateString() },
					{"filter.Period.End", payments.Max(p => p.PayedOn).ToShortDateString() }
				});
		}

		public void CancelPayments()
		{
			Session["payments"] = null;
			RedirectToReferrer();
		}

		public void ProcessPayments()
		{
			if (IsPost)
			{
				var file = Request.Files["inputfile"] as HttpPostedFile;
				if (file == null || file.ContentLength == 0)
				{
					PropertyBag["Message"] = Message.Error("Нужно выбрать файл для загрузки");
					return;
				}

				Session["payments"] = Payment.Parse(file.FileName, file.InputStream);
				RedirectToReferrer();
			}
			else
			{
				PropertyBag["payments"] = Session["payments"];
			}
		}

		public void EditTemp(uint id)
		{
			var payment = FindTempPayment(id);
			if (IsPost)
			{
				BindObjectInstance(payment, "payment", AutoLoadBehavior.NullIfInvalidKey);
				payment.UpdateInn();
				Flash["Message"] = Message.Notify("Сохранено");
				RedirectToReferrer();
			}
			else
			{
				PropertyBag["payment"] = payment;
				PropertyBag["recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();
				RenderView("Edit");
			}
		}

		private Payment FindTempPayment(uint id)
		{
			return TempPayments().First(p => p.GetHashCode() == id);
		}

		private List<Payment> TempPayments()
		{
			return (List<Payment>)Session["payments"];
		}

		public void Delete(uint id)
		{
			var payment = Payment.Find(id);
			payment.Cancel();
			RedirectToReferrer();
		}

		public void DeleteTemp(uint id)
		{
			var payment = FindTempPayment(id);
			TempPayments().Remove(payment);
			RedirectToReferrer();
		}

		public void Edit(uint id)
		{
			var payment = Payment.TryFind(id);
			if (IsPost)
			{
				BindObjectInstance(payment, "payment", AutoLoadBehavior.NullIfInvalidKey);
				payment.DoUpdate();
				Flash["Message"] = Message.Notify("Сохранено");
				RedirectToReferrer();
			}
			else
			{
				PropertyBag["payment"] = payment;
				PropertyBag["recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();
			}
		}

		[return: JSONReturnBinder]
		public object SearchPayer(string term)
		{
			return ActiveRecordLinq
				.AsQueryable<Payer>()
				.Where(p => p.Name.Contains(term))
				.Take(20)
				.ToList()
				.Select(p => new {
					id = p.PayerID,
					label = String.Format("[{0}]. {1} ИНН {2}", p.Id, p.Name, p.INN)
				});
		}
	}
}