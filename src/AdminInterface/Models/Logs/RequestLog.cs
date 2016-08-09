using System;
using System.ComponentModel;
using AdminInterface.AbstractModel;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	/// <summary>
	/// Серверный лог, новой версии analit-f
	/// </summary>
	[ActiveRecord(Table = "RequestLogs", Schema = "logs", SchemaAction = "none")]
	public class RequestLog : IPersonLog
	{
		public RequestLog()
		{
		}

		public RequestLog(User user)
		{
			CreatedOn = DateTime.Now;
			User = user;
		}

		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property]
		public virtual int ErrorType { get; set; }

		[Property]
		public virtual string ErrorDescription { get; set; }

		[Property]
		public virtual string Error { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[Property]
		public virtual DateTime CreatedOn { get; set; }

		[Property]
		public virtual string Version { get; set; }

		[Property]
		public virtual bool IsCompleted { get; set; }

		[Property]
		public virtual bool IsFaulted { get; set; }

		[Property]
		public virtual bool IsConfirmed { get; set; }

		[Property]
		public virtual string UpdateType { get; set; }

		[Property]
		public virtual long? Size { get; set; }

		[Property]
		public virtual DateTime? LastSync { get; set; }

		[Property]
		public virtual string RemoteHost { get; set; }

		[Property]
		public virtual string LocalHost { get; set; }

		[Property]
		public virtual string OSVersion { get; set; }

		[Property]
		public virtual string RequestToken { get; set; }

		public virtual UpdateType UpdateTypeEnum
		{
			get
			{
				UpdateType result;
				if (Enum.TryParse(UpdateType, true, out result)) {
					return result;
				}
				return LastSync == null ? Logs.UpdateType.FullUpdate : Logs.UpdateType.Update;
			}
		}

		public virtual bool HaveLog { get; set; }
	}
}