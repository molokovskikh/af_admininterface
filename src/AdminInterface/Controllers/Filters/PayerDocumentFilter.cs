using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
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
		public bool FindActInvoiceIfIds { get; set; }

		public DateTime? CreatedOn { get; set; }

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

		public void PrepareFindActInvoiceIds(string query)
		{
			SearchText = query;
			FindActInvoiceIfIds = true;
		}

		public IList<T> Find<T>(ISession session)
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Payer", "p", JoinType.InnerJoin);

			if (Year != null && Interval != null)
				criteria.Add(Expression.Eq("Period", new Period(Year.Value, Interval.Value)));
			else if (Year != null)
				criteria.Add(Expression.Sql("{alias}.Period like " + String.Format("'{0}-%'", Year)));
			else if (Interval != null)
				criteria.Add(Expression.Sql("{alias}.Period like " + String.Format("'%-{0}'", (int)Interval)));

			if (Region != null) {
				criteria.Add(Subqueries.Exists(DetachedCriteria.For<Client>()
					.SetProjection(Property.ForName("Id"))
					.CreateAlias("Payers", "pc")
					.Add(Expression.Eq("HomeRegion", Region))
					.Add(Expression.EqProperty("pc.Id", "p.Id"))));
			}

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
					criteria.Add(FindActInvoiceIfIds ? Expression.In("Id", ids) : Expression.In("p.Id", ids));
				else
					criteria.Add(Expression.Like("p.Name", SearchText, MatchMode.Anywhere));
			}

			if(CreatedOn != null) {
				criteria.Add(Expression.Ge("CreatedOn", CreatedOn));
			}

			if (typeof(T) == typeof(Act))
				SortKeyMap["Date"] = "ActDate";

			var items = Find<T>(criteria);

			Count = RowsCount;
			Sum = criteria.SetProjection(Projections.Sum("Sum"))
				.GetExecutableCriteria(session)
				.UniqueResult<decimal>();
			return items;
		}
	}
}