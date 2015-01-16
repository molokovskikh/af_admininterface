using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	/// <summary>
	/// Клиентский лог, новой версии analit-f
	/// </summary>
	[ActiveRecord(Table = "ClientAppLogs", Schema = "logs")]
	public class ClientAppLog
	{
		public ClientAppLog()
		{
		}

		[PrimaryKey("Id")]
		public uint Id { get; set; }

		[Property]
		public virtual string Text { get; set; }

		[BelongsTo("UserId"), Description("Клиент")]
		public virtual User User { get; set; }

		[Property]
		public virtual DateTime CreatedOn { get; set; }
	
		[Property]
		public virtual string Version { get; set; }
	}
}