using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.passwordchange")]
	public class PasswordChangeLogEntity : ActiveRecordLinqBase<PasswordChangeLogEntity>
	{
		public PasswordChangeLogEntity()
		{}

		public PasswordChangeLogEntity(string host, string user, string target)
		{
			UserName = user;
			TargetUserName = target;
			LogTime = DateTime.Now;
			ClientHost = host;
		}

		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public string ClientHost { get; set; }

		[Property]
		public DateTime LogTime { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public string TargetUserName { get; set; }

		[Property]
		public int SmtpId { get; set; }

		[Property]
		public string SentTo { get; set; }

		public static IList<PasswordChangeLogEntity> GetByLogin(string login, DateTime beginDate, DateTime endDate)
		{
			return (from log in Queryable
			        where log.TargetUserName == login && log.LogTime >= beginDate && log.LogTime <= endDate
			        orderby log.LogTime
			        select log).ToList();
		}

		public void SetSentTo(int smtpId, string emailsToNotify)
		{
			SmtpId = smtpId;
			SentTo = emailsToNotify;
		}

		public bool IsChangedByOneSelf()
		{
			return UserName.ToLowerInvariant() == TargetUserName.ToLowerInvariant();
		}
	}
}
