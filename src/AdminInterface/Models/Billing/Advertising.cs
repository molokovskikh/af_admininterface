using System;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class Advertising
	{
		public Advertising()
		{
		}

		public Advertising(Payer payer, decimal cost)
		{
			Payer = payer;
			Cost = cost;
		}

		public Advertising(Payer payer)
		{
			Payer = payer;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public DateTime? Begin { get; set; }

		[Property]
		public DateTime? End { get; set; }

		[Property]
		public decimal Cost { get; set; }

		[Property]
		public decimal? PayedSum { get; set; }

		[BelongsTo]
		public Payer Payer { get; set; }

		[BelongsTo]
		public Payment Payment { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public Invoice Invoice { get; set; }

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public Act Act { get; set; }
	}
}