using System.Collections.Generic;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord("Intersection", Schema = "Customers", Lazy = true)]
	public class Intersection
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

		[Property]
		public virtual bool AvailableForClient { get; set; }

		[Property]
		public double PriceMarkup { get; set; }

		[Property]
		public string SupplierClientId { get; set; }

		[Property]
		public string SupplierPaymentId { get; set; }

		[HasMany(ColumnKey = "IntersectionId", Inverse = true, Lazy = true)]
		public IList<AddressIntersection> Addresses { get; set; }
	}

	[ActiveRecord("AddressIntersection", Schema = "Customers", Lazy = true)]
	public class AddressIntersection
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("IntersectionId")]
		public virtual Intersection Intersection { get; set; }

		[Property]
		public virtual string SupplierDeliveryId { get; set; }
	}
}