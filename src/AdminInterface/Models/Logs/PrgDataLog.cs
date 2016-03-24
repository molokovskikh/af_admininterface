using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Schema = "Logs", SchemaAction = "none")]
	public class PrgDataLog
	{
		public PrgDataLog()
		{
		}

		public PrgDataLog(User user, string method)
		{
			User = user;
			StartTime = DateTime.Now;
			MethodName = method;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[Property]
		public virtual DateTime StartTime { get; set; }

		[Property]
		public virtual string MethodName { get; set; }
	}
}