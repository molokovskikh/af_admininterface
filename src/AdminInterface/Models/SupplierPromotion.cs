using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models
{
	[ActiveRecord("SupplierPromotions", Schema = "usersettings")]
	public class SupplierPromotion : ActiveRecordLinqBase<SupplierPromotion>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public bool Enabled { get; set; }

		[Property]
		public bool AgencyDisabled { get; set; }

		[Property, ValidateNonEmpty("Акция должна иметь наименование.")]
		public string Name { get; set; }

		[BelongsTo(Column = "SupplierId"), ValidateNonEmpty]
		public Supplier Supplier { get; set; }

		[Property, ValidateNonEmpty("Аннотация должна быть заполненна.")]
		public string Annotation { get; set; }

		[Property]
		public string PromoFile { get; set; }

		[
		HasAndBelongsToMany(typeof(Catalog),
			ColumnKey = "PromotionId",
			Table = "PromotionCatalogs",
			Schema = "usersettings",
			ColumnRef = "CatalogId"),
		]
		public virtual IList<Catalog> Catalogs { get; set; }

		[Property, ValidateNonEmpty, ValidateDate]
		public DateTime Begin { get; set; }

		[Property, ValidateNonEmpty, ValidateDate]
		public DateTime End { get; set; }

		[Property]
		public ulong RegionMask { get; set; }

		[ValidateSelf]
		public void ValidatePromo(ErrorSummary errors)
		{
			if (Begin < new DateTime(2011, 1, 1))
				errors.RegisterErrorMessage("Begin", "Дата начала должна быть старше 01-01-2011.");
			if (End < new DateTime(2011, 1, 1))
				errors.RegisterErrorMessage("End", "Дата окончания должна быть старше 01-01-2011.");
			if (Begin.Date > End.Date)
				errors.RegisterErrorMessage("Begin", "Дата начала должна быть раньше даты окончания.");
			if (Catalogs == null || Catalogs.Count == 0)
				errors.RegisterErrorMessage("Catalogs", "Список препаратов не может быть пустым.");
		}
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
			if (columnName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return promotions.OrderByDescending(promotion => promotion.Name).ToList();
				return promotions.OrderBy(promotion => promotion.Name).ToList();
			}
			return promotions;
		}
	}

}