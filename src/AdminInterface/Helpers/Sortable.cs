using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;

namespace AdminInterface.Helpers
{
	public class Sortable
	{
		public string SortBy { get; set; }
		public string SortDirection { get; set; }
		public Dictionary<string, string> SortKeyMap { get; protected set; }

		public void ApplySort(DetachedCriteria criteria)
		{
			SortBy = SortKeyMap.Keys.FirstOrDefault(k => String.Equals(k, SortBy, StringComparison.InvariantCultureIgnoreCase));
			if (SortBy == null)
				SortBy = SortKeyMap.Keys.First();

			var property = SortKeyMap[SortBy];

			if (String.Equals(SortDirection, "desc", StringComparison.InvariantCultureIgnoreCase))
				criteria.AddOrder(Order.Desc(property));
			else
				criteria.AddOrder(Order.Asc(property));
		}
	}
}