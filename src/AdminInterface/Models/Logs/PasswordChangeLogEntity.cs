using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.passwordchange")]
	public class PasswordChangeLogEntity : ActiveRecordBase<PasswordChangeLogEntity>
	{
		public PasswordChangeLogEntity()
		{}

		public PasswordChangeLogEntity(string host, string target, string user)
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
			return ActiveRecordMediator<PasswordChangeLogEntity>
				.FindAll(new[] {Order.Asc("LogTime")},
				         Expression.Eq("TargetUserName", login)
				         && Expression.Between("LogTime", beginDate, endDate));
		}

		public void SetSentTo(string additionEmailsToNotify, string clientEmails)
		{
			SentTo = clientEmails;
			if (String.IsNullOrEmpty(SentTo))
				SentTo = additionEmailsToNotify;
			else if (!String.IsNullOrEmpty(additionEmailsToNotify))
				SentTo += ", " + additionEmailsToNotify;
		}

		public bool IsChangedByOneSelf()
		{
			return UserName.ToLowerInvariant() == TargetUserName.ToLowerInvariant();
		}
	}
}
