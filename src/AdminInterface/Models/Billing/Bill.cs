using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Payments", Schema = "Billing", Where = "Move = 1")]
	public class Bill : ActiveRecordBase<Bill>
	{
		public Bill()
		{
			Name = "Продажа";
			Move = MoveTo.FormUs;
		}

		[Property("Name")]
		private string Name { get; set; }

		[Property("Move")]
		private MoveTo Move { get; set; }

		[PrimaryKey]
		public uint Id { get; set; }

		[Property("PayedOn")]
		public DateTime On { get; set; }

		[Property]
		public float Sum { get; set; }

		[BelongsTo(Column = "PayerId")]
		public Payer For { get; set; }
	}
}
