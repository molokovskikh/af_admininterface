using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("General_Reports", Schema = "reports")]
	public class Report : ActiveRecordLinqBase<Report>
	{
		[PrimaryKey("GeneralReportCode")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual bool Allow { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }
	}
}