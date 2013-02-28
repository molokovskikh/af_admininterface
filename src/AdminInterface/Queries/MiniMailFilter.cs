using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models.Documents;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Queries
{
	public class MailItem : BaseItemForTable
	{
		public MailItem(Mail item)
		{
			MailId = item.Id;
			Date = item.LogTime;
			Supplier = item.Supplier.Name;
			Region = item.Supplier.HomeRegion.Name;
			var to = string.Empty;
			var recipients = item.Recipients.GroupBy(r => r.Type);
			foreach (var group in recipients) {
				switch (group.Key) {
					case RecipientType.Address: {
						to += "Адреса:;";
						break;
					}
					case RecipientType.Client: {
						to += "Клиенты:;";
						break;
					}
					case RecipientType.Region: {
						to += "Регионы:;";
					}
						break;
				}
				foreach (var mailRecipient in group) {
					switch (mailRecipient.Type) {
						case RecipientType.Address: {
							to += mailRecipient.Address.Name + ";";
							break;
						}
						case RecipientType.Client: {
							to += mailRecipient.Client.Name + ";";
							break;
						}
						case RecipientType.Region: {
							to += mailRecipient.Region.Name + ";";
							break;
						}
					}
				}
			}
			To = to;
			Subject = item.Subject;
			DeleteLink = string.Format("<a href=':javascript(0)'>Удалить</a>");
			ShowLink = string.Format("<a href=':javascript(0)'>Показать</a>");
		}

		public void SetAddresseeCount(MailCounter counter)
		{
			AddresseeCount = string.Format("{0} ({1}/{2})", counter.AllCount, counter.CommittedCount, counter.NoCommitedCount);
			Counter = counter;
		}

		public uint MailId { get; set; }

		[Display(Name = "Дата", Order = 0)]
		public DateTime Date { get; set; }

		[Display(Name = "Отправитель", Order = 1)]
		public string Supplier { get; set; }

		[Display(Name = "Дом. регион отправителя", Order = 2)]
		public string Region { get; set; }

		private string _to;

		[Display(Name = "Получатель", Order = 3)]
		public string To {
			get
			{
				if (_to.Count(c => c == ';') > 1)
					return _to.Replace(";", "<br/>");
				return _to.Replace(";", string.Empty);
			}
			set { _to = value; }
		}

		[Display(Name = "Тема", Order = 4)]
		public string Subject { get; set; }

		[Display(Name = "Общее количество получателей", Order = 5)]
		public string AddresseeCount { get; set; }

		public MailCounter Counter { get; set; }

		[Display(Name = "Удалить", Order = 7)]
		public string DeleteLink { get; set; }

		[Display(Name = "Показать", Order = 6)]
		public string ShowLink { get; set; }
	}

	public class MailCounter
	{
		public uint MailId { get; set; }
		public int AllCount { get; set; }
		public int CommittedCount { get; set; }
		public int NoCommitedCount { get; set; }
	}

	public class MiniMailFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public DatePeriod Period { get; set; }
		public uint SupplierId { get; set; }
		public Region Region { get; set; }
		public string SupplierName { get; set; }

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public MiniMailFilter()
		{
			Period = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now);
			LoadDefault = true;
			SortKeyMap = new Dictionary<string, string> {
				{ "Date", "Date" },
				{ "Supplier", "Supplier" },
				{ "Region", "Region" },
				{ "Subject", "Subject" },
				{ "AddresseeCount", "AddresseeCount" }
			};
		}

		private IList<MailItem> Sort(IList<MailItem> query)
		{
			Func<MailItem, object> func = null;
			if (SortBy == "Date")
				func = mail => mail.Date;

			if (SortBy == "Supplier")
				func = mail => mail.Supplier;

			if (SortBy == "Region")
				func = mail => mail.Region;

			if (SortBy == "Subject")
				func = mail => mail.Subject;

			if (SortBy == "AddresseeCount")
				func = mail => mail.Counter.AllCount;

			if (func != null) {
				if (SortDirection == "asc")
					return query.OrderBy(func).ToList();
				return query.OrderByDescending(func).ToList();
			}
			return query.OrderBy(q => q.Date).ToList();
		}

		public IList<BaseItemForTable> Find()
		{
			var mailsQuery = Session.Query<Mail>().Where(m => m.LogTime >= Period.Begin && m.LogTime <= Period.End);

			if (Region != null)
				mailsQuery = mailsQuery.Where(m => m.Logs.Any(l => (l.User.WorkRegionMask & Region.Id) > 0));

			if (SupplierId > 0 && !string.IsNullOrEmpty(SupplierName))
				mailsQuery = mailsQuery.Where(m => m.Supplier.Id == SupplierId);

			var mails = mailsQuery
				.FetchMany(m => m.Recipients)
				.ThenFetch(r => r.Region)
				.Fetch(m => m.Supplier)
				.ThenFetch(s => s.HomeRegion).ToList();

			var result = mails.Select(m => new MailItem(m)).ToList();

			if (mails.Count > 0) {
				var counters = Session.CreateSQLQuery(@"
SELECT m.Id as MailId, count(m.id) as AllCount, count(if (ms.Committed, 1, null)) as CommittedCount, count(if (not ms.Committed, 1, null)) as NoCommitedCount
FROM documents.Mails M
join `logs`.MailSendLogs ms on ms.mailid = m.id
where m.id in (:mails)
group by m.id;")
					.SetParameterList("mails", mails.Select(m => m.Id).ToList())
					.ToList<MailCounter>()
					.ToDictionary(k => k.MailId);

				foreach (var mailItem in result) {
					mailItem.SetAddresseeCount(counters[mailItem.MailId]);
				}
			}

			return Sort(result).Cast<BaseItemForTable>().ToList();
		}
	}
}