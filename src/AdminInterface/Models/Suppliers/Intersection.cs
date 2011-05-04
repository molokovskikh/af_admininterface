using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord("Intersection", Schema = "Future", Lazy = true)]
	public class Intersection : ActiveRecordLinqBase<Intersection>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("ClientId")]
		public virtual Client Client { get; set; }

		[BelongsTo("RegionId")]
		public virtual Region Region { get; set; }

		[BelongsTo("PriceId")]
		public virtual Price Price { get; set; }

		[BelongsTo("LegalEntityId")]
		public virtual LegalEntity Org { get; set; }

		[BelongsTo("CostId")]
		public virtual Cost Cost { get; set; }
	}
}