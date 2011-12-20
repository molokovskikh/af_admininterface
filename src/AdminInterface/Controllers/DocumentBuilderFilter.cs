using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public class DocumentBuilderFilter
	{
		public uint[] PayerId { get; set; }
		public Period Period { get; set; }
		public Region Region { get; set; }
		public Recipient Recipient { get; set; }

		public DocumentBuilderFilter()
		{
			Period = new Period();
		}

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			criteria.Add(Expression.Eq("Period", Period));

			if (PayerId != null && PayerId.Length > 0)
			{
				criteria.Add(Expression.In("p.PayerID", PayerId));
			}
			else
			{
				if (Region != null)
					criteria.CreateCriteria("p.Clients", "c")
						.Add(Expression.Sql("{alias}.RegionCode = " + Region.Id));
				/*.Add(Expression.Eq("c.HomeRegion", Region))*/

				if (Recipient != null)
					criteria.Add(Expression.Eq("Recipient", Recipient));
			}

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

		public PayerDocumentFilter ToDocumentFilter()
		{
			var filter = new PayerDocumentFilter {
				Year = Period.Year,
				Interval = Period.Interval,
				Region = Region,
				Recipient = Recipient
			};

			if (PayerId != null && PayerId.Length > 0)
				filter.SearchText = PayerId.Implode();
			return filter;
		}
	}
}