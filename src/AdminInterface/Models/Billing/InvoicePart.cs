using System;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "billing")]
	public class InvoicePart
	{
		public InvoicePart()
		{}

		public InvoicePart(Invoice invoice)
		{
			Invoice = Invoice;
			PayDate = invoice.Date;
		}

		public InvoicePart(Invoice invoice, string name, decimal cost, int count, DateTime payDate)
		{
			Invoice = invoice;
			Name = name;
			Cost = cost;
			Count = count;
			PayDate = payDate;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, ValidateNonEmpty]
		public virtual string Name { get; set; }

		[Property, ValidateGreaterThanZero]
		public virtual decimal Cost { get; set; }

		[Property, ValidateGreaterThanZero]
		public virtual int Count { get; set; }

		[Property]
		public virtual DateTime PayDate { get; set; }

		[Property]
		public virtual bool Processed { get; set; }

		[BelongsTo]
		public virtual Invoice Invoice { get; set; }

		[BelongsTo]
		public virtual Advertising Ad { get; set; }

		public virtual decimal Sum
		{
			get
			{
				return Cost * Count;
			}
		}

		public virtual void Process()
		{
			Processed = true;
			Invoice.CalculateSum();
		}

		public override string ToString()
		{
			return String.Format("{0} цена {1} количество {2}", Name, Cost, Count);
		}
	}
}