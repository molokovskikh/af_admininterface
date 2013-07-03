using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using NHibernate;

namespace AdminInterface.Models.Billing
{
	public class AuditLogRecord
	{
		// Идентификатор объекта, которому соответствует эта запись (например, идентификатор пользователя или адреса доставки)
		public uint ObjectId { get; set; }

		public DateTime LogTime { get; set; }

		public LogObjectType LogType { get; set; }

		public string OperatorName { get; set; }

		public string Name { get; set; }

		public string Message { get; set; }

		public string Comment { get; set; }

		public bool ShowOnlyPayer { get; set; }

		private static AuditLogRecord GetLogRecord(AddressLogRecord addressLogRecord)
		{
			var log = new AuditLogRecord {
				ObjectId = addressLogRecord.Address.Id,
				LogTime = addressLogRecord.LogTime,
				LogType = LogObjectType.Address,
				OperatorName = addressLogRecord.OperatorName,
				Message = ViewHelper.HumanReadableStatus(addressLogRecord.Enabled),
				Name = addressLogRecord.Address.Value,
				Comment = addressLogRecord.Comment
			};
			return log;
		}

		public static IList<AuditLogRecord> GetLogs(ISession session, Payer payer, bool showOtherRecords)
		{
			var addressLogs = AddressLogRecord.GetLogs(session, payer.Addresses);
			var auditLogs = PayerAuditRecord.Find(payer);

			var logs = addressLogs.Select(log => GetLogRecord(log));
			logs = logs.Concat(auditLogs.Select(r => r.ToAuditRecord()));
			if (showOtherRecords)
				logs = logs.Concat(payer.GetAuditLogs());
			logs = logs.OrderByDescending(r => r.LogTime).ToList();

			var operators = logs.Select(l => l.OperatorName).Distinct().ToList();
			var admins = ActiveRecordLinqBase<Administrator>.Queryable
				.Where(a => operators.Contains(a.UserName))
				.ToList()
				.GroupBy(a => a.UserName.ToLowerInvariant())
				.Select(g => g.First())
				.ToDictionary(a => a.UserName.ToLowerInvariant());
			foreach (var log in logs) {
				var key = log.OperatorName.ToLowerInvariant();
				if (admins.ContainsKey(key)) {
					var administrator = admins[key];
					log.OperatorName = administrator.ManagerName;
				}
			}
			return logs.ToList();
		}
	}
}