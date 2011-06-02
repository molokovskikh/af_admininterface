using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Billing
{
	public enum SwitchLogType
	{
		[Description("Клиент")] ClientLog = 0,
		[Description("Пользователь")] UserLog = 1,
		[Description("Адрес")] AddressLog = 2,
	}

	public class SwitchLogRecord
	{
		// Идентификатор объекта, которому соответствует эта запись (например, идентификатор пользователя или адреса доставки)
		public uint ObjectId { get; set; }

		public DateTime LogTime { get; set; }

		public SwitchLogType LogType { get; set; }

		public string OperatorName { get; set; }

		public bool Status { get; set; }

		public string Value { get; set; }

		public static IList<SwitchLogRecord> GetUnionLogs(IList<ClientLogRecord> clientLogs,
			IList<UserLogRecord> userLogs,
			IList<AddressLogRecord> addressLogs)
		{
			var logs = clientLogs.Select(log => GetLogRecord(log)).ToList();
			logs.AddRange(userLogs.Select(log => GetLogRecord(log)));
			logs.AddRange(addressLogs.Select(log => GetLogRecord(log)));
			return logs.OrderByDescending(log => log.LogTime).ToList();
		}

		private static SwitchLogRecord GetLogRecord(ClientLogRecord clientLogRecord)
		{
			var log = new SwitchLogRecord {
				LogTime = clientLogRecord.LogTime,
				LogType = SwitchLogType.ClientLog,
				OperatorName = clientLogRecord.OperatorName,
				Status = clientLogRecord.ClientStatus.HasValue && clientLogRecord.ClientStatus.Value.Equals(ClientStatus.On),
				Value = clientLogRecord.Client.Name,
			};
			return log;
		}

		private static SwitchLogRecord GetLogRecord(AddressLogRecord addressLogRecord)
		{
			var log = new SwitchLogRecord {
				ObjectId = addressLogRecord.Address.Id,
				LogTime = addressLogRecord.LogTime,
				LogType = SwitchLogType.AddressLog,
				OperatorName = addressLogRecord.OperatorName,
				Status = addressLogRecord.Enabled,
				Value = addressLogRecord.Address.Value,
			};
			return log;
		}

		private static SwitchLogRecord GetLogRecord(UserLogRecord userLogRecord)
		{
			var log = new SwitchLogRecord {
				ObjectId = userLogRecord.User.Id,
				LogTime = userLogRecord.LogTime,
				LogType = SwitchLogType.UserLog,
				OperatorName = userLogRecord.OperatorName,
				Status = userLogRecord.Enabled.HasValue && userLogRecord.Enabled.Value,
				Value = userLogRecord.User.GetLoginOrName(),
			};
			return log;
		}

		public static IList<SwitchLogRecord> GetLogs(Payer payer)
		{
			var userLogs = UserLogRecord.GetUserEnabledLogRecords(payer.Users);
			var addressLogs = AddressLogRecord.GetAddressLogRecords(payer.Addresses);
			var clientLogs = ClientLogRecord.GetClientLogRecords(payer.Clients);

			var logs = GetUnionLogs(clientLogs, userLogs, addressLogs);
			var operators = logs.Select(l => l.OperatorName).Distinct().ToList();
			var admins = ActiveRecordLinqBase<Administrator>.Queryable
				.Where(a => operators.Contains(a.UserName))
				.ToList()
				.GroupBy(a => a.UserName.ToLowerInvariant())
				.Select(g => g.First())
				.ToDictionary(a => a.UserName.ToLowerInvariant());
			foreach (var log in logs)
			{
				var key = log.OperatorName.ToLowerInvariant();
				if (admins.ContainsKey(key))
				{
					var administrator = admins[key];
					log.OperatorName = administrator.ManagerName;
				}
			}
			return logs;
		}
	}
}
