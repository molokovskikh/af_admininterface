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

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }
	}
}