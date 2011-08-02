using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;

namespace AdminInterface.Helpers
{
	public class Sortable
	{
		public string SortBy { get; set; }
		public string SortDirection { get; set; }
		public Dictionary<string, string> SortKeyMap { get; protected set; }

		public void ApplySort(ICriteria query)
		{
			var property = GetSortProperty();

			if (String.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase))
				query.AddOrder(Order.Desc(property));
			else
				query.AddOrder(Order.Asc(property));
		}

		public void ApplySort(DetachedCriteria criteria)
		{
			var property = GetSortProperty();

			if (String.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase))
				criteria.AddOrder(Order.Desc(property));
			else
				criteria.AddOrder(Order.Asc(property));
		}

		private string GetSortProperty()
		{
			SortBy = SortKeyMap.Keys.FirstOrDefault(k => String.Equals(k, SortBy, StringComparison.InvariantCultureIgnoreCase));
			if (SortBy == null)
				SortBy = SortKeyMap.Keys.First();

			var property = SortKeyMap[SortBy];
			return property;
		}
	}
}