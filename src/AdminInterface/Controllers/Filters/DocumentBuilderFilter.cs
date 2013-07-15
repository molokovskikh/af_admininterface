using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers.Filters
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

			criteria.Add(Restrictions.Eq("Period", Period));

			if (PayerId != null && PayerId.Length > 0) {
				criteria.Add(Restrictions.In("p.Id", PayerId));
			}
			else {
				if (Region != null)
					criteria.CreateCriteria("p.Clients", "c")
						.Add(Expression.Sql("{alias}.RegionCode = " + Region.Id));
				/*.Add(Expression.Eq("c.HomeRegion", Region))*/

				if (Recipient != null)
					criteria.Add(Restrictions.Eq("Recipient", Recipient));
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

		public List<Invoice> BuildInvoices(DateTime invoiceDate)
		{
			return ArHelper.WithSession(session => {
				var invoicePeriod = Period.GetInvoicePeriod();
				IList<Payer> payers;
				var periodEnd = Period.GetPeriodEnd();
				if (PayerId == null || PayerId.Length == 0) {
					var query = session.QueryOver<Payer>()
						.Where(p => p.AutoInvoice == InvoiceType.Auto
							&& p.PayCycle == invoicePeriod
								&& p.Recipient != null);

					if (Recipient != null)
						query.Where(p => p.Recipient == Recipient);

					query.Where(p => p.Registration.RegistrationDate <= periodEnd);
					query.OrderBy(p => p.Name);
					payers = query.List<Payer>();

					if (Region != null)
						payers = payers.Where(p => p.Clients.Any(c => c.HomeRegion == Region)).ToList();
				}
				else {
					payers = session.Query<Payer>()
						.Where(p => PayerId.Contains(p.Id)
							&& p.Registration.RegistrationDate <= periodEnd
								&& p.Recipient != null)
						.OrderBy(p => p.Name)
						.ToList();
				}

				var invoices = new List<Invoice>();
				foreach (var payer in payers) {
					if (session.Query<Invoice>().Any(i => i.Payer == payer && i.Period == Period))
						continue;

					var minPeriod = session.Query<Invoice>()
						.Where(i => i.Payer == payer)
						.Select(i => i.Period)
						.Distinct()
						.ToList()
						.Select(p => p.GetPeriodBegin())
						.DefaultIfEmpty()
						.Min();

					if (Period.GetPeriodBegin() < minPeriod)
						continue;

					foreach (var invoice in payer.BuildInvoices(invoiceDate, Period).Where(i => i.Sum > 0)) {
						invoices.Add(invoice);
						session.Save(invoice);
					}
				}
				return invoices;
			});
		}
	}
}