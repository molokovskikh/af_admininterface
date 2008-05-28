using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "billing.payers")]
	public class PaymentInstance : ActiveRecordBase<PaymentInstance>
	{
		[PrimaryKey(Column = "payerId")]
		public uint Id { get; set; }

		[Property(Column = "oldpaydate")]
		public DateTime PayDate { get; set; }

		[Property(Column = "oldtariff")]
		public uint PaySum { get; set; }
	}
}