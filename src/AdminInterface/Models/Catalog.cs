using System.Collections.Generic;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("catalogs.catalog")]
	public class Catalog : ActiveRecordBase<Catalog>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public bool Hidden { get; set; }

		[Property]
		public string Name { get; set; }

		[HasMany(ColumnKey = "CatalogId", Inverse = false, Lazy = true)]
		public virtual IList<SupplierPromotion> Promotions { get; set; }
	}
}