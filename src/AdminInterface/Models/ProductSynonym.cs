using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord("Synonym", Schema = "farm")]
	public class ProductSynonym
	{
		public ProductSynonym()
		{
		}

		public ProductSynonym(string synonym)
		{
			Synonym = synonym;
		}

		/// <summary>
		/// Id Синонима. Ключевое поле.
		/// </summary>
		[PrimaryKey]
		public int SynonymCode { get; set; }

		/// <summary>
		/// Продукт
		/// </summary>
		[BelongsTo("ProductId")]
		public Product Product { get; set; }

		/// <summary>
		/// Уцененный
		/// </summary>
		[Property]
		public bool Junk { get; set; }

		/// <summary>
		/// Синоним продукта
		/// </summary>
		[Property]
		public string Synonym { get; set; }

		/// <summary>
		/// Прайс-лист
		/// </summary>
		[BelongsTo("PriceCode")]
		public Price Price { get; set; }

		/// <summary>
		/// Код, присвоенный поставщиком
		/// </summary>
		[Property]
		public string SupplierCode { get; set; }
	}
}