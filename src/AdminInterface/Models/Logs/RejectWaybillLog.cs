using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Schema = "Logs", Lazy = true)]
	public class RejectWaybillLog
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier FromSupplier { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client ForClient { get; set; }

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
	}
}