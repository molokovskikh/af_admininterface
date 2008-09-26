using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Payments", Schema = "Billing", Where = "Move = 0")]
	public class Payment : ActiveRecordBase<Payment>
	{
		public Payment()
		{
			Name = "Оплата";
			Move = MoveTo.ToUs;
		}

		[Property("Name")] 
		private string Name { get; set; }

		[Property("Move")]
		private MoveTo Move { get; set; }

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public DateTime PayedOn { get; set; }

		[Property]
		public float Sum { get; set; }

		[BelongsTo(Column = "PayerId")]
		public Payer PayFrom { get; set; }

		public static DateTime DefaultBeginPeriod(Payer payer)
		{
			if (payer.PayCycle == 0)
				return DateTime.Today.AddMonths(-2);

			return DateTime.Today.AddMonths(-2 * 3);
		}

		public static DateTime DefaultEndPeriod(Payer payer)
		{
			return DateTime.Today;
		}

		public static Payment[] FindBetwen(Payer payer, DateTime form, DateTime to)
		{
			return FindAll(Order.Asc("PayedOn"), Expression.Between("PayedOn", form, to)
			                                     && Expression.Eq("PayFrom", payer));
		}
	}
}
