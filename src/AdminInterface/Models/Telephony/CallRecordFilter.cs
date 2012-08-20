using System;
using System.Collections.Generic;
using Common.MySql;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Telephony
{
	public class CallRecordFilter : Sortable
	{
		private int pageSize;

		public string SearchText { get; set; }
		public CallType? CallType { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }

		public int Page { get; set; }

		public int TotalPages { get; private set; }

		public int PageSize
		{
			get { return pageSize; }
			set { pageSize = value; }
		}

		public CallRecordFilter()
		{
			SortBy = "WriteTime";
			SortDirection = "Desc";
			SortKeyMap = new Dictionary<string, string> {
				{"WriteTime", "WriteTime"},
				{"From", "From"},
				{"NameFrom", "NameFrom"},
				{"To", "To"},
				{"NameTo", "NameTo"},
				{"CallType", "CallType"}
			};

			BeginDate = DateTime.Today.AddDays(-1);
			EndDate = DateTime.Today;
			PageSize = 25;
		}

		public IList<CallRecord> Find()
		{
			var searchText = String.IsNullOrEmpty(SearchText) ? String.Empty : SearchText.ToLower();
			searchText.Trim();
			searchText = Utils.StringToMySqlString(searchText);
			var sortFilter = String.Format(" order by `{0}` {1} ", GetSortProperty(), GetSortDirection());
			var limit = String.Format("limit {0}, {1}", Page * PageSize, PageSize);
			var searchCondition = String.IsNullOrEmpty(searchText) ? String.Empty :
				" and (LOWER({CallRecord}.`From`) like \"%" + searchText +
				"%\" or LOWER({CallRecord}.`To`) like \"%" + searchText +
				"%\" or LOWER({CallRecord}.NameFrom) like \"%" + searchText +
				"%\" or LOWER({CallRecord}.NameTo) like \"%" + searchText + "%\") ";
			if (CallType != null)
				searchCondition += " and {CallRecord}.CallType = " + Convert.ToInt32(CallType);

			IList<CallRecord> callList = null;
			var sql = @"
select {CallRecord.*}
from logs.RecordCalls {CallRecord}
where {CallRecord}.WriteTime > :BeginDate and {CallRecord}.WriteTime < :EndDate" + searchCondition + sortFilter + limit;

			var countSql = @"
select count(*)
from logs.RecordCalls {CallRecord}
where {CallRecord}.WriteTime > :BeginDate and {CallRecord}.WriteTime < :EndDate" + searchCondition;
			countSql = countSql.Replace("{CallRecord}", "c");

			ArHelper.WithSession(session => {
				callList = session.CreateSQLQuery(sql)
					.AddEntity(typeof(CallRecord))
					.SetParameter("BeginDate", BeginDate)
					.SetParameter("EndDate", EndDate.AddDays(1))
					.List<CallRecord>();

				var count = Convert.ToInt32(session.CreateSQLQuery(countSql)
					.SetParameter("BeginDate", BeginDate)
					.SetParameter("EndDate", EndDate.AddDays(1))
					.UniqueResult());
				TotalPages = count / PageSize;
				if (count % PageSize > 0)
					TotalPages++;
			});
			return callList;
		}
	}
}
