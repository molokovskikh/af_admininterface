using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using AdminInterface.Models.Logs;

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
			var logs = new List<SwitchLogRecord>();

			foreach (var log in clientLogs)
				logs.Add(GetLogRecord(log));
			foreach (var log in userLogs)
				logs.Add(GetLogRecord(log));
			foreach (var log in addressLogs)
				logs.Add(GetLogRecord(log));

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
	}
}
