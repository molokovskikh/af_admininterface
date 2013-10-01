using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Schema = "logs")]
	public class SupplierLog : ActiveRecordLinqBase<SupplierLog>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[BelongsTo("SupplierId")]
		public virtual Supplier Supplier { get; set; }

		[Property]
		public virtual bool? Disabled { get; set; }

		[Property]
		public virtual string Comment { get; set; }
	}
}