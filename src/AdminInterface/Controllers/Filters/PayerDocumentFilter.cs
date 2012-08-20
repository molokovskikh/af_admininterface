using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers.Filters
{
	public class PayerDocumentFilter : PaginableSortable
	{
		[Description("Выберите год:")]
		public int? Year { get; set; }

		[Description("Выберите период:")]
		public Interval? Interval { get; set; }

		public Region Region { get; set; }
		public Recipient Recipient { get; set; }
		public string SearchText { get; set; }

		public int Count { get; private set; }
		public decimal Sum { get; private set; }

		public PayerDocumentFilter()
		{
			SortBy = "PayerName";
			SortKeyMap = new Dictionary<string, string> {
				{ "Id", "Id" },
				{ "Payer", "Payer" },
				{ "Recipient", "Recipient" },
				{ "PayerName", "p.Name" },
				{ "Sum", "Sum" },
				{ "Period", "Period" },
				{ "Date", "Date" }
			};
		}

		public IList<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			if (Year != null && Interval != null)
				criteria.Add(Expression.Eq("Period", new Period(Year.Value, Interval.Value)));
			else if (Year != null)
				criteria.Add(Expression.Sql("{alias}.Period like " + String.Format("'{0}-%'", Year)));
			else if (Interval != null)
				criteria.Add(Expression.Sql("{alias}.Period like " + String.Format("'%-{0}'", (int)Interval)));

			if (Region != null)
				criteria.CreateCriteria("p.Clients", "c")
					.Add(Expression.Sql("{alias}.RegionCode = " + Region.Id)
					/*Expression.Eq("c.HomeRegion", Region)*/);

			if (Recipient != null)
				criteria.Add(Expression.Eq("Recipient", Recipient));

			if (!String.IsNullOrEmpty(SearchText)) {
				var parts = SearchText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				var ids = parts
					.Select(p => {
						uint id;
						uint.TryParse(p, out id);
						return id;
					})
					.Where(p => p > 0)
					.ToArray();

				if (ids.Length == parts.Length)
					criteria.Add(Expression.In("p.PayerID", ids));
				else
					criteria.Add(Expression.Like("p.Name", SearchText, MatchMode.Anywhere));
			}

			if (typeof(T) == typeof(Act))
				SortKeyMap["Date"] = "ActDate";

			var items = Find<T>(criteria);

			var docs = items
				.GroupBy(i => ((dynamic)i).Id)
				.Select(g => g.First())
				.ToList();

			Count = RowsCount;
			Sum = ArHelper.WithSession(s => {
				criteria.SetProjection(Projections.Sum("Sum"));
				return criteria
					.GetExecutableCriteria(s)
					.UniqueResult<decimal>();
			});
			return docs;
		}
	}
}