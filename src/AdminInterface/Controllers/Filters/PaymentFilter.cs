using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Controllers.Filters
{
	public class PaymentFilter : Sortable, SortableContributor
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

			SortKeyMap = new Dictionary<string, string> {
				{"Recipient", "Recipient"},
				{"PayedOn", "PayedOn"},
				{"Payer", "Payer"},
				{"Sum", "Sum"},
				{"RegistredOn", "RegistredOn"}
			};
		}

		public List<Recipient> Recipients
		{
			get { return Recipient.Queryable.OrderBy(r => r.Name).ToList(); }
		}

		public List<Payment> Find()
		{
			var criteria = DetachedCriteria.For<Payment>()
				.Add(Expression.Ge("PayedOn", Period.Begin) && Expression.Lt("PayedOn", Period.End.AddDays(1)));

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			if (ShowOnlyUnknown)
				criteria.Add(Expression.IsNull("Payer"));

			if (!String.IsNullOrWhiteSpace(SearchText))
			{
				criteria.CreateAlias("Payer", "p");
				criteria.Add(Expression.Like("p.Name", SearchText));
			}

			ApplySort(criteria);

			return ArHelper.WithSession(s =>
				criteria.GetExecutableCriteria(s)
					.List<Payment>()).ToList();
		}

		public string GetUri()
		{
			var parts = new List<string>();
			if (Period != null)
				parts.Add(String.Format("filter.Period.Begin={0}&filter.Period.End={1}",
					Period.Begin.ToShortDateString(),
					Period.End.ToShortDateString()));
			if (Recipient != null)
				parts.Add(String.Format("filter.Recipient.Id={0}", Recipient.Id));
			if (SearchText != null)
				parts.Add(String.Format("filter.SearchText={0}", SearchText));
			if (ShowOnlyUnknown)
				parts.Add(String.Format("filter.ShowOnlyUnknown={0}", ShowOnlyUnknown));
			return parts.ToArray().Implode("&");
		}
	}
}