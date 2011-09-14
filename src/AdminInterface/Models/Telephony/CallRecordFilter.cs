using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;

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
			pageSize = 25;
		}

		public IList<CallRecord> Find()
		{
			var searchText = String.IsNullOrEmpty(SearchText) ? String.Empty : SearchText.ToLower();
			searchText.Trim();
			searchText = Utils.StringToMySqlString(searchText);
			var sortFilter = String.Format(" order by `{0}` {1} ", GetSortProperty(), GetSortDirection());
			var limit = String.Format("limit {0}, {1}", Page * pageSize, pageSize);
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
				TotalPages = count / pageSize;
				if (count % pageSize > 0)
					TotalPages++;
			});
			return callList;
		}
	}
}
