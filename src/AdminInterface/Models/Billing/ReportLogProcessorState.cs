using System;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class ReportLogProcessorState : ActiveRecordLinqBase<ReportLogProcessorState>
	{
		[PrimaryKey]
		public int Id { get; set; }

		[Property]
		public DateTime LastRun { get; set; }
	}
}