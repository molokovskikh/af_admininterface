using System;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord("DocumentHeaders", Schema = "documents")]
	public class FullDocument : Document
	{
		public FullDocument()
		{ }

		public FullDocument(DocumentReceiveLog log)
		{
			Log = log;
			WriteTime = DateTime.Now;
			Supplier = log.FromSupplier;
			ClientCode = log.ForClient.Id;
			AddressId = log.Address.Id;
		}

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		[BelongsTo("DownloadId")]
		public virtual DocumentReceiveLog Log { get; set; }
	}
}
