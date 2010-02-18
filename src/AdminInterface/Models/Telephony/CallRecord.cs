using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Telephony
{
	[ActiveRecord("logs.RecordCalls")]
	public class CallRecord : ActiveRecordBase<CallRecord>
	{
		private static string[] _sortableColumns = new[] { "WriteTime", "WriteTime", "From", "To" };

		[PrimaryKey]
		public virtual ulong Id { get; set; }

		[Property]
		public virtual string From { get; set; }

		[Property]
		public virtual string To { get; set; }

		[Property]
		public virtual DateTime WriteTime { get; set; }

		public static IList<CallRecord> GetByPeriod(DateTime beginDate, DateTime endDate, int sortColumnIndex, bool usePaging, int currentPage, int pageSize)
		{
			var index = Math.Abs(sortColumnIndex) - 1;
			var sortFilter = String.Format(" order by `{0}` ", _sortableColumns[index]);
			sortFilter += (sortColumnIndex > 0) ? " asc " : " desc ";
			var limit = usePaging ? String.Format("limit {0}, {1}", currentPage * pageSize, pageSize) : String.Empty;

			IList<CallRecord> callList = null;
			var sql = @"
select {CallRecord.*}
from logs.RecordCalls {CallRecord}
where {CallRecord}.WriteTime > :BeginDate and {CallRecord}.WriteTime < :EndDate " + sortFilter + limit;
			ArHelper.WithSession(session => {
				callList = session.CreateSQLQuery(sql)
					.AddEntity(typeof(CallRecord))
					.SetParameter("BeginDate", beginDate)
					.SetParameter("EndDate", endDate)
					.List<CallRecord>();
            });
			return callList;
		}
	}
}
