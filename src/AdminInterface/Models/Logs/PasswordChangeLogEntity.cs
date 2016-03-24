using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("passwordchange", Schema = "logs", SchemaAction = "none")]
	public class PasswordChangeLogEntity
	{
		public PasswordChangeLogEntity()
		{
		}

		public PasswordChangeLogEntity(string target)
		{
			UserName = SecurityContext.Administrator.UserName;
			ClientHost = SecurityContext.Administrator.Host;
			TargetUserName = target;
			LogTime = DateTime.Now;
		}

		// Используется только в тесте
		public PasswordChangeLogEntity(string target, string userName, string hostName)
		{
			UserName = userName;
			ClientHost = hostName;
			TargetUserName = target;
			LogTime = DateTime.Now;
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

		[Property]
		public string SmsLog { get; set; }

		public static IList<PasswordChangeLogEntity> GetByLogin(string login, DateTime beginDate, DateTime endDate)
		{
			IList<PasswordChangeLogEntity> entity = null;
			ArHelper.WithSession(session => entity = session.CreateSQLQuery(@"
select {PasswordChangeLogEntity.*}
from logs.passwordchange {PasswordChangeLogEntity}
where LogTime >= :BeginDate and LogTime <= :EndDate and TargetUserName = :Login")
				.AddEntity(typeof(PasswordChangeLogEntity))
				.SetParameter("BeginDate", beginDate)
				.SetParameter("EndDate", endDate)
				.SetParameter("Login", login).List<PasswordChangeLogEntity>());
			return entity;
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