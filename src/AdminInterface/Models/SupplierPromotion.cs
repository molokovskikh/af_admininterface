using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord("supplierpromotions", Schema = "usersettings")]
	public class SupplierPromotion : ActiveRecordLinqBase<SupplierPromotion>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public bool Enabled { get; set; }

		[Property]
		public bool AgencyDisabled { get; set; }

		[BelongsTo(Column = "SupplierId"), ValidateNonEmpty]
		public Supplier Supplier { get; set; }

		//[BelongsTo(Column = "CatalogId"), ValidateNonEmpty]
		public Catalog Catalog { get; set; }

		[Property, ValidateNonEmpty("Аннотация должна быть заполненна.")]
		public string Annotation { get; set; }

		[Property]
		public string PromoFile { get; set; }
	}

	public static class SupplierPromotionExtension
	{
		public static List<SupplierPromotion> SortBy(this List<SupplierPromotion> promotions, string columnName, bool descending)
		{
			if (String.IsNullOrEmpty(columnName))
				return promotions;
			if (columnName.Equals("Id", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return promotions.OrderByDescending(promotion => promotion.Id).ToList();
				return promotions.OrderBy(promotion => promotion.Id).ToList();
			}
			if (columnName.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return promotions.OrderByDescending(promotion => promotion.Enabled).ToList();
				return promotions.OrderBy(promotion => promotion.Enabled).ToList();
			}
			if (columnName.Equals("CatalogName", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return promotions.OrderByDescending(promotion => promotion.Catalog.Name).ToList();
				return promotions.OrderBy(promotion => promotion.Catalog.Name).ToList();
			}
			return promotions;
		}
	}

}