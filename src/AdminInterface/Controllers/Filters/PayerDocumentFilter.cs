using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers.Filters
{
	public class PayerDocumentFilter : Sortable, SortableContributor, IUrlContributor
	{
		public Period? Period { get; set; }
		public Region Region { get; set; }
		public Recipient Recipient { get; set; }
		public string SearchText { get; set; }

		public int Count { get; private set; }
		public decimal Sum { get; private set; }

		public PayerDocumentFilter()
		{
			SortBy = "PayerName";
			SortKeyMap = new Dictionary<string, string> {
				{"Id", "Id"},
				{"Payer", "Payer"},
				{"Recipient", "Recipient"},
				{"PayerName", "p.Name"},
				{"Sum", "Sum"},
				{"Period", "Period"},
				{"Date", "Date"}
			};
		}

		public IList<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			if (Period != null)
				criteria.Add(Expression.Eq("Period", Period));

			if (Region != null)
				criteria.CreateCriteria("p.Clients", "c")
					.Add(Expression.Sql("{alias}.RegionCode = " + Region.Id)
					/*Expression.Eq("c.HomeRegion", Region)*/);

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("p.Name", SearchText, MatchMode.Anywhere));
			if (typeof(T) == typeof(Act))
				SortKeyMap["Date"] = "ActDate";

			ApplySort(criteria);

			var docs = ArHelper.WithSession(s => criteria
				.GetExecutableCriteria(s).List<T>())
				.GroupBy(i => ((dynamic)i).Id)
				.Select(g => g.First())
				.ToList();
			
			Count = docs.Count;
			Sum = docs.Sum(d => (decimal)((dynamic)d).Sum);
			return docs;
		}

		public string[] ToUrl()
		{
			var parts = new List<string>();
			if (Period != null)
				parts.Add(String.Format("filter.Period={0}", (int)Period));
			if (Region != null)
				parts.Add(String.Format("filter.Region.Id={0}", Region.Id));
			if (Recipient != null)
				parts.Add(String.Format("filter.Recipient.Id={0}", Recipient.Id));
			if (SearchText != null)
				parts.Add(String.Format("filter.SearchText={0}", SearchText));
			return parts.ToArray();
		}

		public string ToUrlPart()
		{
			return ToUrl().Implode("&");
		}

		public string GetUri()
		{
			return ToUrlPart();
		}

		public IDictionary GetQueryString()
		{
			var parts = new Dictionary<string, object>();
			if (Period != null)
				parts.Add("filter.Period", Period);
			if (Region != null)
				parts.Add("filter.Region.Id", Region.Id);
			if (Recipient != null)
				parts.Add("filter.Recipient.Id", Recipient.Id);
			if (SearchText != null)
				parts.Add("filter.SearchText", SearchText);
			if (SortBy != null)
				parts.Add("filter.SortBy", SortBy);
			if (SortDirection != null)
				parts.Add("filter.SortDirection", SortDirection);
			return parts;
		}
	}
}