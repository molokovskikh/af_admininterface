using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Criterion;

namespace AdminInterface.Controllers
{
	public class AdvertisingFilter
	{
		[Description("Показать только не размещенную:")]
		public bool ShowWithoutDates { get; set; }

		[Description("Показать только неоплаченную:")]
		public bool ShowWithoutPayment { get; set; }

		public List<Advertising> Filter(ISession session)
		{
			var criteria = DetachedCriteria.For<Advertising>();

			if (ShowWithoutDates)
				criteria.Add(Expression.IsNull("Begin"));

			if (ShowWithoutPayment)
				criteria.Add(Expression.IsNull("Payment"));

			criteria.AddOrder(Order.Asc("Begin"));

			return criteria
				.GetExecutableCriteria(session)
				.List<Advertising>()
				.ToList();
		}
	}

	public class AdvertisingsController : AdminInterfaceController
	{
		public void Index([DataBind("filter")] AdvertisingFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["ads"] = filter.Filter(DbSession);
		}

		public void BuildInvoice(uint id)
		{
			var ad = DbSession.Load<Advertising>(id);
			if (ad.Invoice != null) {
				Error("Для рекламы уже сформирован счет");
				RedirectToReferrer();
				return;
			}
			ad.Invoice = new Invoice(ad);
			DbSession.Save(ad);
			DbSession.Flush();

			RedirectTo(ad.Invoice, "Print");
		}

		public void BuildAct(uint id)
		{
			var ad = DbSession.Load<Advertising>(id);
			if (ad.Act != null) {
				Error("Для рекламы уже сформирован счет");
				RedirectToReferrer();
				return;
			}
			var invoice = ad.Invoice;
			if (invoice == null)
				invoice = new Invoice(ad);

			ad.Act = new Act(invoice.Date, invoice);
			DbSession.Save(ad);
			DbSession.Flush();

			RedirectTo(ad.Act, "Print");
		}

		public void Delete(uint id)
		{
			var ad = DbSession.Load<Advertising>(id);
			DbSession.Delete(ad);
			Notify("Удалено");
			RedirectToReferrer();
		}

		public void Edit(uint id)
		{
			RenderView("/Payers/NewAd");

			var ad = DbSession.Load<Advertising>(id);
			if (IsPost) {
				BindObjectInstance(ad, "ad");
				if (!HasValidationError(ad)) {
					DbSession.Save(ad);
					Redirect("Advertisings", "Index");
				}
			}
			PropertyBag["payer"] = ad.Payer;
			PropertyBag["ad"] = ad;
		}
	}
}