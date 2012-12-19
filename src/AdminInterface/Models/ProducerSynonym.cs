using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("SynonymFirmCr", Schema = "farm")]
	public class ProducerSynonym
	{
		public ProducerSynonym()
		{
		}

		public ProducerSynonym(string synonym)
		{
			Synonym = synonym;
		}

		/// <summary>
		/// Id Синонима. Ключевое поле.
		/// </summary>
		[PrimaryKey]
		public int SynonymFirmCrCode { get; set; }

		/// <summary>
		/// Синоним производителя
		/// </summary>
		[Property]
		public string Synonym { get; set; }

		/// <summary>
		/// Прайс-лист
		/// </summary>
		[BelongsTo("PriceCode")]
		public Price Price { get; set; }

		/// <summary>
		/// Код производителя ProducerId
		/// </summary>
		[Property]
		public int? CodeFirmCr { get; set; }

		/// <summary>
		/// Код, присвоенный поставщиком
		/// </summary>
		[Property]
		public string SupplierCode { get; set; }
	}
}