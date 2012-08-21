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

		private static AuditLogRecord GetLogRecord(ClientLogRecord clientLogRecord)
		{
			var log = new AuditLogRecord {
				ObjectId = clientLogRecord.Client.Id,
				LogTime = clientLogRecord.LogTime,
				LogType = LogObjectType.Client,
				OperatorName = clientLogRecord.OperatorName,
				Message = ViewHelper.HumanReadableStatus(clientLogRecord.ClientStatus.HasValue && clientLogRecord.ClientStatus.Value.Equals(ClientStatus.On)),
				Name = clientLogRecord.Client.Name,
				Comment = clientLogRecord.Comment
			};
			return log;
		}

		private static AuditLogRecord GetLogRecord(SupplierLog supplierLog)
		{
			var log = new AuditLogRecord {
				ObjectId = supplierLog.Supplier.Id,
				LogTime = supplierLog.LogTime,
				LogType = LogObjectType.Supplier,
				OperatorName = supplierLog.OperatorName,
				Message = ViewHelper.HumanReadableStatus(!supplierLog.Disabled.Value),
				Name = supplierLog.Supplier.Name,
				Comment = supplierLog.Comment
			};
			return log;
		}

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

		private static AuditLogRecord GetLogRecord(UserLogRecord userLogRecord)
		{
			var log = new AuditLogRecord {
				ObjectId = userLogRecord.User.Id,
				LogTime = userLogRecord.LogTime,
				LogType = LogObjectType.User,
				OperatorName = userLogRecord.OperatorName,
				Message = ViewHelper.HumanReadableStatus(userLogRecord.Enabled.HasValue && userLogRecord.Enabled.Value),
				Name = userLogRecord.User.GetLoginOrName(),
				Comment = userLogRecord.Comment
			};
			return log;
		}

		public static IList<AuditLogRecord> GetLogs(Payer payer)
		{
			var userLogs = UserLogRecord.GetLogs(payer.Users);
			var addressLogs = AddressLogRecord.GetLogs(payer.Addresses);
			var clientLogs = ClientLogRecord.GetLogs(payer.Clients);
			var supplierLogs = SupplierLog.GetLogs(payer.Suppliers);
			var auditLogs = PayerAuditRecord.Find(payer);

			var logs = clientLogs.Select(log => GetLogRecord(log))
				.Concat(userLogs.Select(log => GetLogRecord(log)))
				.Concat(addressLogs.Select(log => GetLogRecord(log)))
				.Concat(supplierLogs.Select(log => GetLogRecord(log)))
				.Concat(auditLogs.Select(r => r.ToAuditRecord()))
				.OrderByDescending(r => r.LogTime)
				.ToList();

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
			return logs;
		}
	}
}