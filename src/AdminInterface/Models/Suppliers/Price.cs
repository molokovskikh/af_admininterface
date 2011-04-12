using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord("PricesData", Schema = "Usersettings", Lazy = true)]
	public class Price : ActiveRecordLinqBase<Price>
	{
		[PrimaryKey("PriceCode")]
		public virtual uint Id { get; set; }

		[Property("PriceName")]
		public virtual string Name { get; set; }

		[Property]
		public virtual int? PriceType { get; set; }

		[Property]
		public virtual int? CostType { get; set; }

		[Property]
		public virtual bool AgencyEnabled { get; set; }

		[Property]
		public virtual bool Enabled { get; set; }

		[Property]
		public virtual decimal UpCost { get; set; }

		[Property]
		public virtual bool AlowInt { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }
	}
}