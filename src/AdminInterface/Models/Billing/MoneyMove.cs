using System;
using System.Globalization;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	public enum MoveTo
	{
		ToUs = 0,
		FormUs = 1,
	}

	[ActiveRecord("Billing.Payments")]
	public class MoneyMove : ActiveRecordBase<MoneyMove>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public float Sum { get; set; }

		[Property("PayedOn")]
		public DateTime On { get; set; }

		[BelongsTo(Column = "PayerID")]
		public Payer For { get; set; }

		[Property]
		public MoveTo Move { get; set; }

		[Property]
		public string Name { get; set; }

		public string Debit
		{
			get
			{
				if (Move == MoveTo.FormUs)
					return Sum.ToString();
				return "";
			}
		}

		public string Credit
		{
			get
			{
				if (Move == MoveTo.ToUs)
					return Sum.ToString();
				return "";
			}
		}
	}
}
