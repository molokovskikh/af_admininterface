using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class PaymentStatistics
	{
		public PaymentStatistics(IList<Payment> payments)
		{
			Count = payments.Count;
			Sum = payments.Sum(p => p.Sum);
		}

		public int Count { get; set; }
		public decimal Sum { get; set; }
	}

	public class SessionExpiredException : Exception
	{}

	[
		Rescue(typeof(PaymentsController), "SessionExpired", typeof(SessionExpiredException)),
		Helper(typeof(PaginatorHelper), "paginator"),
	]
	public class PaymentsController : AdminInterfaceController
	{
		public PaymentsController()
		{
			SetARDataBinder();
		}

		public void Index([SmartBinder] PaymentFilter filter)
		{
			var payments = filter.Find();
			PropertyBag["filter"] = filter;
			PropertyBag["payments"] = payments;
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
				PropertyBag["recipients"] = DbSession.Query<Recipient>().OrderBy(r => r.Name).ToList();
				PropertyBag["payments"] = Payment.Queryable
					.Where(p => p.RegistredOn >= DateTime.Today)
					.OrderBy(p => p.RegistredOn).ToList();
			}
		}

		public void SavePayments()
		{
			var payments = TempPayments();
			Session["payments"] = null;
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
				PropertyBag["recipients"] = DbSession.Query<Recipient>().OrderBy(r => r.Name).ToList();
				RenderView("Edit");
			}
		}

		public void SessionExpired()
		{
			Flash["Message"] = Message.Error("Время сессии истекло. Загрузите выписку повторно.");
			RedirectToAction("ProcessPayments");
		}

		private Payment FindTempPayment(uint id)
		{
			return TempPayments().First(p => p.GetHashCode() == id);
		}

		private List<Payment> TempPayments()
		{
			var payments = (List<Payment>)Session["payments"];
			if (payments == null)
				throw new SessionExpiredException();
			return payments;
		}

		public void Delete(uint id)
		{
			var payment = Payment.Find(id);
			payment.Delete();
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
				PropertyBag["recipients"] = DbSession.Query<Recipient>().OrderBy(r => r.Name).ToList();
			}
		}

		[return: JSONReturnBinder]
		public object SearchPayer(string term)
		{
			uint id;
			uint.TryParse(term, out id);
			return ActiveRecordLinq
				.AsQueryable<Payer>()
				.Where(p => p.Name.Contains(term) || p.PayerID == id)
				.Take(20)
				.ToList()
				.Select(p => new {
					id = p.PayerID,
					label = String.Format("[{0}]. {1} ИНН {2}", p.Id, p.Name, p.INN)
				});
		}
	}
}