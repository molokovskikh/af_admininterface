using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models
{
	/// <summary>
	/// Заголовок документа отказа. Содержит строки отказа.
	/// В целом, представляет собой не только заголовок, но и сам документ отказа.
	/// </summary>
	[ActiveRecord(Schema = "Documents")]
	public class RejectHeader
	{
		public RejectHeader()
		{
			Lines = new List<RejectLine>();
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime WriteTime { get; set; }

		[Property]
		public virtual string Parser { get; set; }

		[BelongsTo("SupplierId")]
		public virtual Supplier Supplier { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[BelongsTo("DownloadId")]
		public virtual DocumentReceiveLog Log { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public virtual IList<RejectLine> Lines { get; set; }

	}

	/// <summary>
	/// Строка документа отказа.
	/// </summary>
	[ActiveRecord(Schema = "Documents")]
	public class RejectLine
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		//Обязательное поле
		[Property, Description("Код товара")]
		public virtual string Code { get; set; }

		[Property, Description("Наименование товара")]
		public virtual string Product { get; set; }

		[Property, Description("Производитель товара")]
		public virtual string Producer { get; set; }

		[Property, Description("Количество заказанных товаров")]
		public virtual uint? Ordered { get; set; }

		//Обязательное поле
		[Property, Description("Количество отказов по товару")]
		public virtual uint Rejected { get; set; }

		[Property, Description("Стоимость товара")]
		public virtual decimal? Cost { get; set; }

		[Property, Description("Код производителя, строка макс 255 символов")]
		public virtual string CodeCr { get; set; }

		[Property, Description("Номер заявки АналитФАРМАЦИЯ")]
		public virtual uint OrderId { get; set; }

		[BelongsTo("HeaderId")]
		public virtual RejectHeader Header { get; set; }

		[BelongsTo("ProductId")]
		public virtual Product ProductEntity { get; set; }

		[BelongsTo("ProducerId")]
		public virtual Producer ProducerEntity { get; set; }
	}
}