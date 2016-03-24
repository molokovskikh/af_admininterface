using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	public enum RejectReasonType
	{
		[Description("Нет причины")] NoReason = 0,
		[Description("Адрес отключен")] AddressDisable = 1,
		[Description("Клиент отключен")] ClientDisable = 2,
		[Description("Адрес не доступен поставщику")] AddressNoAvailable = 3,
		[Description("Пользователь не обновлялся более месяца")] UserNotUpdate = 4
	}

	[ActiveRecord(Schema = "Logs", Lazy = true, SchemaAction = "none")]
	public class RejectWaybillLog
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier FromSupplier { get; set; }

		[BelongsTo("ClientCode")]
		public virtual ClientForReading ForClient { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string FileName { get; set; }

		[Property]
		public virtual string Addition { get; set; }

		[Property]
		public virtual ulong DocumentSize { get; set; }

		[Property]
		public virtual RejectReasonType RejectReason { get; set; }
	}
}