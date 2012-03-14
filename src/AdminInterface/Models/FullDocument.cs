using System;
using System.Collections.Generic;
using System.IO;
using AddUser;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord("DocumentHeaders", Schema = "documents")]
	public class FullDocument : Common.Web.Ui.Models.Document
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
