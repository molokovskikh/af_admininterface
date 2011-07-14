using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public class DocumentBuilderFilter
	{
		public Period Period { get; set; }
		public Region Region { get; set; }
		public Recipient Recipient { get; set; }

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			criteria.Add(Expression.Eq("Period", Period));

			if (Region != null)
				criteria.CreateCriteria("p.Clients", "c")
					.Add(Expression.Sql("{alias}.RegionCode = " + Region.Id));
			/*.Add(Expression.Eq("c.HomeRegion", Region))*/

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			List<T> items = null;

			ArHelper.WithSession(s => {
				items = criteria
					.GetExecutableCriteria(s).List<T>()
					.GroupBy(i => ((dynamic)i).Id)
					.Select(g => g.First())
					.ToList();
			});
			return items;
		}

		public Dictionary<string, string> ToUrl()
		{
			var map = new Dictionary<string, string> {
				{"filter.Period", Period.ToString()},
			};
			if (Region != null)
				map.Add("filter.Region.Id", Region.Id.ToString());

			if (Recipient != null)
				map.Add("filter.Recipient.Id", Recipient.Id.ToString());
			return map;
		}
	}
}