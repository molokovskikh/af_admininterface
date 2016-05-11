using System;
using System.ComponentModel;
using System.Web;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models
{
	[ActiveRecord("waybill_sources", Schema = "Documents"), Auditable]
	public class WaybillSource : BaseWaybillSource, IAuditable
	{
		public WaybillSource()
		{
		}

		public WaybillSource(Supplier supplier)
		{
			Supplier = supplier;
		}

		[PrimaryKey("FirmCode", Generator = PrimaryKeyType.Foreign)]
		public override uint Id { get; set; }

		[OneToOne]
		public virtual Supplier Supplier { get; set; }

		public IAuditRecord GetAuditRecord()
		{
			return new AuditRecord(Supplier);
		}
	}
}
