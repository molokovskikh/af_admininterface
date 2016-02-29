using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using AddUser;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Common.Tools;
using Common.Web.Ui.Models;
using NHibernate;

namespace AdminInterface.Models.Logs
{
	public enum DocumentType
	{
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Reject = 2,
		[Description("Документы от АналитФармация")] InforoomDoc = 3
	}

	[ActiveRecord("DocumentSendLogs", Schema = "Logs", Lazy = true)]
	public class DocumentSendLog
	{
		public DocumentSendLog()
		{
		}

		public DocumentSendLog(User user, DocumentReceiveLog document)
		{
			ForUser = user;
			Received = document;
			Committed = true;
			FileDelivered = true;
			DocumentDelivered = true;
		}

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

		[Property]
		public virtual bool FileDelivered { get; set; }

		[Property]
		public virtual bool DocumentDelivered { get; set; }

		[Property]
		public virtual DateTime? SendDate { get; set; }

		private bool DocumentProcessedSuccessfully()
		{
			return (Id <= 15374942) || FileDelivered || DocumentDelivered;
		}

		public virtual DateTime? GetDisplayRequestTime()
		{
			if (DocumentProcessedSuccessfully() && SendedInUpdate != null)
				return SendedInUpdate.RequestTime;
			return null;
		}
	}

	[ActiveRecord("Document_logs", Schema = "Logs", Lazy = true)]
	public class DocumentReceiveLog
	{
		public DocumentReceiveLog()
		{
		}

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

		[Property]
		public virtual bool IsFake { get; set; }

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

			var file = $"{Id}_{FileHelper.StringToFileName(FromSupplier.Name)}" +
				$"({Path.GetFileNameWithoutExtension(FileName)}){Path.GetExtension(FileName)}";
			file = Path.Combine(config.AptBox, Address.Id.ToString(), DocumentType + "s", file);
			if (File.Exists(file))
				return file;
			return Directory.GetFiles(Path.Combine(config.AptBox, Address.Id.ToString(), DocumentType + "s"), $"{Id}_*")
				.FirstOrDefault();
		}
	}
}