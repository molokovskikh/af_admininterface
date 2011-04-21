using System;
using System.ComponentModel;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	public enum DocumentType
	{
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Reject = 2,
		[Description("Документы от АК \"Инфорум\"")] InforoomDoc = 3
	}

	[ActiveRecord("DocumentSendLogs", Schema = "Logs")]
	public class DocumentSendLog : ActiveRecordBase<DocumentSendLog>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User ForUser { get; set; }

		[BelongsTo("DocumentId")]
		public virtual DocumentReceiveLog Received { get; set; }

		[BelongsTo("UpdateId")]
		public virtual UpdateLogEntity SendedInUpdate { get; set; }

		[Property]
		public virtual bool Committed { get; set; }
	}

	[ActiveRecord("Document_logs", Schema = "Logs")]
	public class DocumentReceiveLog : ActiveRecordBase<DocumentReceiveLog>
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier FromSupplier { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client ForClient { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[OneToOne(PropertyRef = "Log")]
		public virtual Document Document { get; set; }

		[BelongsTo("SendUpdateId")]
		public virtual UpdateLogEntity SendUpdateLogEntity { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string FileName { get; set; }

		[Property]
		public virtual string Addition { get; set; }

		[Property]
		public virtual ulong DocumentSize { get; set; }

		[Property]
		public virtual DocumentType DocumentType { get; set; }
	}
}