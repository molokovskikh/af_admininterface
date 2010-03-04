using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using AdminInterface.Helpers;
using Common.Web.Ui.Models;
using NHibernate.Criterion;
using AdminInterface.Models.Security;
using NHibernate;
using NHibernate.Transform;
using Common.MySql;

namespace AdminInterface.Models.Telephony
{
	[ActiveRecord("logs.RecordCalls")]
	public class CallRecord : ActiveRecordBase<CallRecord>
	{
		private IList<CallRecordFile> _files = null;

		private static string[] _sortableColumns = new[] { "WriteTime", "WriteTime", "From", "NameFrom", "To", "NameTo" };

		[PrimaryKey]
		public virtual ulong Id { get; set; }

		[Property]
		public virtual string From { get; set; }

		[Property]
		public virtual string To { get; set; }

		[Property]
		public virtual DateTime WriteTime { get; set; }

		[Property(Column = "NameFrom")]
		public virtual string NameSource { get; set; }

		[Property(Column = "NameTo")]
		public virtual string NameDestination { get; set; }

		public virtual IList<CallRecordFile> Files
		{
			get
			{
				if (_files == null)
				{
					_files = new List<CallRecordFile>();
					var searchPattern = String.Format("{0}*", Id);
					var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);
					foreach (var file in files)
						_files.Add(new CallRecordFile(file));
				}
				return _files;
			}
		}

		public static IList<CallRecord> GetByPeriod(DateTime beginDate, DateTime endDate, int sortColumnIndex,
			bool usePaging, int currentPage, int pageSize, string searchText)
		{
			if (!String.IsNullOrEmpty(searchText))
			{
				searchText.Trim();
				searchText = Utils.StringToMySqlString(searchText);
			}
			var index = Math.Abs(sortColumnIndex) - 1;
			var sortFilter = String.Format(" order by `{0}` ", _sortableColumns[index]);
			sortFilter += (sortColumnIndex > 0) ? " asc " : " desc ";
			var limit = usePaging ? String.Format("limit {0}, {1}", currentPage * pageSize, pageSize) : String.Empty;
			var searchCondition = String.IsNullOrEmpty(searchText) ? String.Empty :
				" and (LOWER({CallRecord}.`From`) like \"%" + searchText.ToLower() +
				"%\" or LOWER({CallRecord}.`To`) like \"%" + searchText.ToLower() +
                "%\" or LOWER({CallRecord}.NameFrom) like \"%" + searchText.ToLower() +
				"%\" or LOWER({CallRecord}.NameTo) like \"%" + searchText.ToLower() + "%\") ";

			IList<CallRecord> callList = null;
			var sql = @"
select {CallRecord.*}
from logs.RecordCalls {CallRecord}
where {CallRecord}.WriteTime > :BeginDate and {CallRecord}.WriteTime < :EndDate" + searchCondition + sortFilter + limit;
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
