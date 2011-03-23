using System.Collections.Generic;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("Catalog", Schema = "catalogs")]
	public class Catalog : ActiveRecordBase<Catalog>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public bool Hidden { get; set; }

		[Property]
		public string Name { get; set; }

		[HasAndBelongsToMany(typeof(SupplierPromotion),
			Lazy = true,
			Inverse=true,
			ColumnKey = "CatalogId",
			Table = "PromotionCatalogs",
			Schema = "usersettings",
			ColumnRef = "PromotionId")]
		public virtual IList<SupplierPromotion> Promotions { get; set; }
	}
}