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
	public class PaymentFilter : PaginableSortable
	{
		public Recipient Recipient { get; set; }
		public DatePeriod Period { get; set; }
		public string SearchText { get; set; }
		public bool ShowOnlyUnknown { get; set; }
		public decimal Sum { get; private set; }
		public int Count { get; private set; }

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

		public IList<Payment> Find()
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

			var payments = Find<Payment>(criteria);

			Sum = ArHelper.WithSession(s =>
				criteria.GetExecutableCriteria(s)
					.SetProjection(Projections.Sum("Sum"))
					.UniqueResult<decimal>());
			Count = RowsCount;

			return payments;
		}
	}
}