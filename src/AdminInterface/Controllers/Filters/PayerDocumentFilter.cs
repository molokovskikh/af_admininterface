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
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers.Filters
{
	public class PayerDocumentFilter : Sortable
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
	}
}