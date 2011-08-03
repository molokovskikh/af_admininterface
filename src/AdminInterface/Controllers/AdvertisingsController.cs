using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using AppHelper = AdminInterface.Helpers.AppHelper;

namespace AdminInterface.Controllers
{
	public class AdvertisingFilter
	{
		public bool ShowWithoutDates { get; set; }
		public bool ShowWithoutPayment { get; set; }

		public List<Advertising> Filter()
		{
			var criteria = DetachedCriteria.For<Advertising>();

			if (ShowWithoutDates)
				criteria.Add(Expression.IsNull("Begin"));

			if (ShowWithoutPayment)
				criteria.Add(Expression.IsNull("Payment") /*|| Expression.g*/);

			criteria.AddOrder(Order.Asc("Begin"));

			return ArHelper.WithSession(s => criteria
				.GetExecutableCriteria(s).List<Advertising>())
				.ToList();
		}
	}

	public class AdvertisingsController : SmartDispatcherController
	{
		public void Index([DataBind("filter")] AdvertisingFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["ads"] = filter.Filter();
		}

		public void BuildInvoice(uint id)
		{
			var ad = Advertising.Find(id);
			if (ad.Invoice != null)
			{
				Flash["Message"] = Message.Error("Для рекламы уже сформирован счет");
				RedirectToReferrer();
			}
			ad.Invoice = new Invoice(ad);
			ad.UpdateAndFlush();

			var app = new AppHelper(Context);
			RedirectToUrl(app.GetUrl(ad.Invoice, "Print"));
		}

		public void BuildAct(uint id)
		{
			var ad = Advertising.Find(id);
			if (ad.Act != null)
			{
				Flash["Message"] = Message.Error("Для рекламы уже сформирован счет");
				RedirectToReferrer();
			}
			var invoice = ad.Invoice;
			if (invoice == null)
				invoice = new Invoice(ad);

			ad.Act = new Act(invoice.Date, invoice);
			ad.UpdateAndFlush();

			var app = new AppHelper(Context);
			RedirectToUrl(app.GetUrl(ad.Act, "Print"));
		}

		public void Delete(uint id)
		{
			var ad = Advertising.Find(id);
			ad.Delete();
			Flash["Message"] = Message.Notify("Удалено");
			RedirectToReferrer();
		}

		public void Edit(uint id)
		{
			Binder.Validator = Validator;
			RenderView("/Payers/NewAd");

			var ad = Advertising.Find(id);
			if (IsPost)
			{
				BindObjectInstance(ad, "ad");
				if (!HasValidationError(ad))
				{
					ad.Save();
					Redirect("Advertisings", "Index");
				}
			}
			PropertyBag["payer"] = ad.Payer;
			PropertyBag["ad"] = ad;
		}
	}
}