using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models
{
	[ActiveRecord("smart_order_rules", Schema = "ordersendrules", Lazy = true)]
	public class SmartOrderRules : ActiveRecordLinqBase<SmartOrderRules>
	{
		[PrimaryKey("Id")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string ParseAlgorithm { get; set; }

		[Property]
		public virtual uint? AssortimentPriceCode { get; set; }

		public static SmartOrderRules TestSmartOrder()
		{
			var testOrder = new SmartOrderRules {
#if !DEBUG
				AssortimentPriceCode = 4662,
#endif
				ParseAlgorithm = "TestSource",
			};
			return testOrder;
		}

		public virtual string GetAssortimentPriceName()
		{
			if (AssortimentPriceCode.HasValue) {
				var price = ActiveRecordMediator<Price>.FindByPrimaryKey(AssortimentPriceCode.Value);
				return string.Format("{0} - {1}", price.Supplier.Name, price.Name);
			}
			return string.Empty;
		}
	}
}