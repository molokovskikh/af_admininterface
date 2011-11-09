using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Helpers;
using Castle.ActiveRecord.Framework;
using Common.MySql;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	public enum AccountingSearchBy
	{
		[Description("Автоматически")] Auto,
		[Description("По адресу")] ByAddress,
		[Description("По пользователю")] ByUser,
		[Description("По клиенту")] ByClient,
		[Description("По плательщику")] ByPayer,
		[Description("По отчету")] ByReport,
		[Description("По поставщику")] BySupplier,
	}

	public class AccountFilter
	{
		public AccountFilter()
		{
			BeginDate = DateTime.Today.AddDays(-1);
			EndDate = DateTime.Today;
		}

		public string SearchText { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }
		public AccountingSearchBy SearchBy { get; set; }

		public IList<Account> Find(Pager pager)
		{
			if (String.IsNullOrEmpty(SearchText))
				SearchText = "";

			var searchText = String.Format("%{0}%", Utils.StringToMySqlString(SearchText));
			var searchNumber = 0;
			Int32.TryParse(SearchText, out searchNumber);

			var filter = "(date(c.WriteTime) >= :BeginDate and date(c.WriteTime) <= :EndDate) and c.BeAccounted = 1";
			var from = "";
			var where = "";
			var order = "ORDER BY c.WriteTime DESC";
			var limit = String.Format(" LIMIT {0}, {1} ", pager.Page * pager.PageSize, pager.PageSize);
			switch (SearchBy)
			{
				case AccountingSearchBy.ByAddress:
					from = @"
	join Future.Addresses a ON a.AccountingId = c.Id AND c.Type = 1";
					where = @"
(a.Address LIKE :SearchText OR a.Id = :SearchNumber)";
					break;
				case AccountingSearchBy.ByClient:
					from = @"
	join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0
		join Future.Clients cl ON cl.Id = u.ClientId";
					where = "cl.Name LIKE :SearchText OR cl.FullName LIKE :SearchText OR cl.Id = :SearchNumber";
					break;
				case AccountingSearchBy.ByPayer:
					from = @"
	join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0
		join Future.Clients cl ON cl.Id = u.ClientId
			join Billing.PayerClients pc on pc.ClientId = cl.Id
				join Billing.Payers p ON pc.PayerId = p.PayerId";
					where = "p.PayerId = :SearchNumber OR p.ShortName LIKE :SearchText OR p.JuridicalName LIKE :SearchText";
					break;
				case AccountingSearchBy.ByUser:
					from = "join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0";
					where = "(ifnull(u.Name, '') LIKE :SearchText OR u.Id = :SearchNumber)";
					break;
				case AccountingSearchBy.ByReport:
					from = "join Reports.general_reports gr on gr.GeneralReportCode = c.ObjectId and c.Type = 2";
					where = "(gr.Comment LIKE :SearchText OR gr.GeneralReportCode = :SearchNumber)";
					break;
				case AccountingSearchBy.BySupplier:
					from = "join Future.Suppliers s on s.Id = c.ObjectId and c.Type = 3";
					where = "(s.Name LIKE :SearchText or s.FullName like :SearchText or s.Id = :SearchNumber)";
					break;
				case AccountingSearchBy.Auto:
					from = @"
	left join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0
		left join Future.Clients  cl ON cl.Id = u.ClientId
			left join Billing.PayerClients pc on pc.ClientId = cl.Id
				left join Billing.Payers p ON pc.PayerId = p.PayerId
	left join Future.Addresses a ON a.AccountingId = c.Id AND c.Type = 1
	left join Reports.general_reports gr on gr.GeneralReportCode = c.ObjectId and c.Type = 2
	left join Future.Suppliers s on s.Id = c.ObjectId and c.Type = 3";
					where = @"(
	(u.Id is not null and (ifnull(u.Name, '') LIKE :SearchText OR u.Id = :SearchNumber)) or 
	(a.Address LIKE :SearchText or a.Id = :SearchNumber) or
	(p.PayerId = :SearchNumber or p.ShortName LIKE :SearchText or p.JuridicalName LIKE :SearchText) or
	(gr.Comment LIKE :SearchText or gr.GeneralReportCode = :SearchNumber) or
	(s.Name LIKE :SearchText or s.FullName like :SearchText or s.Id = :SearchNumber)
)";
					break;
				default:
					return Enumerable.Empty<Account>().ToList();
			}

			return ArHelper.WithSession(session => {
				pager.Total = Convert.ToInt32(session.CreateSQLQuery(String.Format(@"
SELECT count(*)
from Billing.Accounts c
{0}
WHERE {1} and {2}
", @from, @where, filter))
					.SetParameter("BeginDate", BeginDate)
					.SetParameter("EndDate", EndDate)
					.SetParameter("SearchText", searchText)
					.SetParameter("SearchNumber", searchNumber)
					.UniqueResult());

				var items = session.CreateSQLQuery(String.Format(@"
SELECT c.Id
from Billing.Accounts c
{0}
WHERE {1} and {2}
{3}
{4}
", @from, @where, filter, order, limit))
					.SetParameter("BeginDate", BeginDate)
					.SetParameter("EndDate", EndDate)
					.SetParameter("SearchText", searchText)
					.SetParameter("SearchNumber", searchNumber)
					.List();
				var ids = items.Cast<object>().Select(Convert.ToUInt32).ToArray();
				return ActiveRecordLinqBase<Account>.Queryable.Where(a => ids.Contains(a.Id)).OrderByDescending(a => a.WriteTime).ToList();
			});
		}
	}
}