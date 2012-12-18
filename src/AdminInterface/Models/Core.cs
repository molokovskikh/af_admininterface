using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord("Core0", Schema = "Farm")]
	public class Core
	{
		[PrimaryKey]
		public ulong Id { get; set; }

		[Property]
		public string Quantity { get; set; }

		[BelongsTo("PriceCode")]
		public Price Price { get; set; }

		[BelongsTo("SynonymCode")]
		public ProductSynonym ProductSynonym { get; set; }

		[BelongsTo("SynonymFirmCrCode")]
		public ProducerSynonym ProducerSynonym { get; set; }

		[Property]
		public int? ProductId { get; set; }

		[Property]
		public int? CodeFirmCr { get; set; }

		[Property]
		public string Code { get; set; }

		[Property]
		public string CodeCr { get; set; }
	}
}