using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("General_Reports", Schema = "reports"), Auditable]
	public class Report : IAuditable
	{
		[PrimaryKey("GeneralReportCode")]
		public virtual uint Id { get; set; }

		[Property, Auditable("Включен")]
		public virtual bool Allow { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }

		public virtual string ChangeComment { get; set; }

		public IAuditRecord GetAuditRecord()
		{
			return new PayerAuditRecord(Payer, "$$$", ChangeComment) { ObjectType = LogObjectType.Report };
		}
	}
}