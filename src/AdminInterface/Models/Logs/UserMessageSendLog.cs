using System;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Schema = "Logs")]
	public class UserMessageSendLog
	{
		public UserMessageSendLog()
		{}

		public UserMessageSendLog(UserMessage message)
		{
			LogTime = DateTime.Now;
			User = message.User;
			Admin = SecurityContext.Administrator;
			Message = message.Message;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[BelongsTo]
		public virtual User User { get; set; }

		[BelongsTo]
		public virtual Administrator Admin { get; set; }

		[Property]
		public virtual string Message { get; set; }
	}
}