using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

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

	[Layout("GeneralWithJQueryOnly")]
	public class AdvertisingsController : SmartDispatcherController
	{
		public void Index([DataBind("filter")] AdvertisingFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["ads"] = filter.Filter();
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