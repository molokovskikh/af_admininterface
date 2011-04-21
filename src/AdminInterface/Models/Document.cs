using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models
{
	[ActiveRecord("DocumentHeaders", Schema = "documents")]
	public class Document : ActiveRecordLinqBase<Document>
	{
		public Document()
		{ }

		public Document(DocumentReceiveLog log)
		{
			Log = log;
			WriteTime = DateTime.Now;
			Supplier = log.FromSupplier;
			ClientCode = log.ForClient.Id;
			//AddressId = log.AddressId;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		//[Property]
		//public uint DownloadId { get; set; }

		[Property]
		public virtual DateTime WriteTime { get; set; }

		[Property]
		public virtual uint FirmCode { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		[Property]
		public virtual uint ClientCode { get; set; }

		[Property]
		public virtual uint? AddressId { get; set; }

		[Property]
		public virtual string ProviderDocumentId { get; set; }

		[Property]
		public virtual DateTime? DocumentDate { get; set; }

		[BelongsTo("DownloadId")]
		public virtual DocumentReceiveLog Log { get; set; }

		[HasMany(ColumnKey = "DocumentId", Cascade = ManyRelationCascadeEnum.All, Inverse = true, Lazy = true)]
		public virtual IList<DocumentLine> Lines { get; set; }

		public DocumentLine NewLine()
		{
			return NewLine(new DocumentLine());
		}

		public DocumentLine NewLine(DocumentLine line)
		{
			if (Lines == null)
				Lines = new List<DocumentLine>();

			line.Document = this;
			Lines.Add(line);
			return line;
		}
	}

	[ActiveRecord("DocumentBodies", Schema = "documents")]
	public class DocumentLine : ActiveRecordValidationBase<DocumentLine>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo("DocumentId")]
		public Document Document { get; set; }

		[Property]
		public string Product { get; set; }

		[Property]
		public string Code { get; set; }

		[Property]
		public string Certificates { get; set; }

		[Property]
		public string Period { get; set; }

		[Property]
		public string Producer { get; set; }

		[Property]
		public string Country { get; set; }

		[Property]
		public decimal? ProducerCost { get; set; }

		[Property]
		public decimal? RegistryCost { get; set; }

		[Property]
		public decimal? SupplierPriceMarkup { get; set; }

		[Property]
		public uint? Nds { get; set; }

		[Property]
		public decimal? SupplierCostWithoutNDS { get; set; }

		[Property]
		public decimal? SupplierCost { get; set; }

		[Property]
		public uint? Quantity { get; set; }

		[Property]
		public bool? VitallyImportant { get; set; }

		[Property]
		public string SerialNumber { get; set; }

		public void SetNds(decimal nds)
		{
			SupplierCostWithoutNDS = null;
			if (SupplierCost.HasValue)
				SupplierCostWithoutNDS = Math.Round(SupplierCost.Value / (1 + nds / 100), 2);
			Nds = (uint?)nds;
		}

		public void SetProducerCostWithoutNds(decimal cost)
		{
			SupplierCostWithoutNDS = cost;
			Nds = null;
			if (SupplierCost.HasValue && SupplierCostWithoutNDS.HasValue)
				Nds = (uint?)(Math.Round((SupplierCost.Value / SupplierCostWithoutNDS.Value - 1) * 100));
		}

		public void SetSupplierCostByNds(decimal? nds)
		{
			Nds = (uint?)nds;
			SupplierCost = null;
			if (SupplierCostWithoutNDS.HasValue && Nds.HasValue)
				SupplierCost = Math.Round(SupplierCostWithoutNDS.Value * (1 + ((decimal)Nds.Value / 100)), 2);
		}
	}
}
