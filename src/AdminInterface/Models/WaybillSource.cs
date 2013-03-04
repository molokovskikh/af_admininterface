using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	public enum WaybillSourceType
	{
		[Description("FTP АК 'Инфорум'")] FtpInforoom = 4,
		[Description("FTP Поставщика")] FtpSupplier = 5,
		[Description("HTTP")] Http = 2,
		[Description("Email")] Email = 1,
	}

	[ActiveRecord("waybill_sources", Schema = "Documents")]
	public class WaybillSource
	{
		[PrimaryKey("FirmCode", Generator = PrimaryKeyType.Foreign)]
		public virtual uint Id { get; set; }

		[OneToOne]
		public virtual Supplier Supplier { get; set; }

		[Property]
		public virtual string EMailFrom { get; set; }

		[Property("SourceID")]
		public virtual WaybillSourceType SourceType { get; set; }

		[Property]
		public virtual string ReaderClassName { get; set; }

		[Property]
		public virtual string WaybillUrl { get; set; }

		[Property]
		public virtual string RejectUrl { get; set; }

		[Property]
		public virtual string UserName { get; set; }

		[Property]
		public virtual string Password { get; set; }

		[Property]
		public virtual uint? DownloadInterval { get; set; }

		[Property]
		public virtual bool FtpActiveMode { get; set; }

		public virtual IList<string> GetEmailsList()
		{
			if (string.IsNullOrEmpty(EMailFrom))
				return new List<string>();
			return EMailFrom.Split(new[] { ',' }).Select(s => s.Trim()).ToList();
		}
	}
}