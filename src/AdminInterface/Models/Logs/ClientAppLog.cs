﻿using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	/// <summary>
	/// Клиентский лог, новой версии analit-f
	/// </summary>
	[ActiveRecord(Table = "ClientAppLogs", Schema = "logs")]
	public class ClientAppLog
	{
		public ClientAppLog(User user)
		{
			CreatedOn = DateTime.Now;
			User = user;
		}

		public ClientAppLog()
		{
		}

		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property(Lazy = true)]
		public virtual string Text { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[Property]
		public virtual DateTime CreatedOn { get; set; }

		[Property]
		public virtual string Version { get; set; }

		[Property]
		public virtual string RequestToken { get; set; }

		public virtual RequestLog ToRequestLog()
		{
			return new RequestLog {
				Id = Id,
				User = User,
				CreatedOn = CreatedOn,
				Version = Version,
				IsConfirmed = true,
				UpdateType = "Logs",
				HaveLog = true,
			};
		}
	}
}