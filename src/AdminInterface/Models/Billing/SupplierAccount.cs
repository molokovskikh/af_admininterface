using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "3")]
	public class SupplierAccount : Account
	{
		public SupplierAccount()
		{}

		public SupplierAccount(Supplier supplier)
		{
			Supplier = supplier;
			if (supplier.Segment == Segment.Retail)
				Payment = 600;
		}

		[BelongsTo("ObjectId")]
		public virtual Supplier Supplier { get; set; }

		public override Payer Payer
		{
			get { return Supplier.Payer; }
		}

		public override string Name
		{
			get { return Supplier.Name; }
		}

		public override LogObjectType ObjectType
		{
			get { return LogObjectType.Supplier; }
		}

		public override bool ShouldPay()
		{
			return Supplier.Enabled && base.ShouldPay();
		}

		public override uint ObjectId
		{
			get { return Supplier.Id; }
		}

		public override string DefaultDescription
		{
			get
			{
				return "Справочно-информационные услуги за {0}";
			}
		}
	}
}