using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using AddUser;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Logs
{
	public enum DocumentType
	{
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Reject = 2,
		[Description("Документы от АК \"Инфорум\"")] InforoomDoc = 3
	}

	[ActiveRecord("DocumentSendLogs", Schema = "Logs", Lazy = true)]
	public class DocumentSendLog : ActiveRecordBase<DocumentSendLog>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User ForUser { get; set; }

		[BelongsTo("DocumentId", Lazy = FetchWhen.OnInvoke)]
		public virtual DocumentReceiveLog Received { get; set; }

		[BelongsTo("UpdateId")]
		public virtual UpdateLogEntity SendedInUpdate { get; set; }

		[Property]
		public virtual bool Committed { get; set; }
	}

	[ActiveRecord("Document_logs", Schema = "Logs", Lazy = true)]
	public class DocumentReceiveLog : ActiveRecordBase<DocumentReceiveLog>
	{
		public DocumentReceiveLog()
		{}

		public DocumentReceiveLog(Supplier supplier)
		{
			FromSupplier = supplier;
			LogTime = DateTime.Now;
			DocumentType = DocumentType.Waybill;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier FromSupplier { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client ForClient { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[OneToOne(PropertyRef = "Log")]
		public virtual FullDocument Document { get; set; }

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

		[HasMany(Inverse = true, Lazy = true)]
		public virtual IList<DocumentSendLog> SendLogs { get; set; }

		public virtual string GetRemoteFileName(AppConfig config)
		{
			if (String.IsNullOrEmpty(FileName))
				return null;
			if (FromSupplier == null)
				return null;
			if (Address == null)
				return null;

			var file = String.Format("{0}_{1}({2}){3}", Id, FromSupplier.Name, Path.GetFileNameWithoutExtension(FileName), Path.GetExtension(FileName));
			return Path.Combine(config.AptBox, Address.Id.ToString(), DocumentType.ToString() + "s", file);
		}
	}
}