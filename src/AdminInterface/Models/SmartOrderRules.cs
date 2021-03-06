﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models.Audit;
using System.Linq;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace AdminInterface.Models
{
	public enum LoaderType
	{
		[Description("Коду")] Code = 0,
		[Description("Коду товара и производителя")] CodeAndCodeCr = 1,
		[Description("Наименованию товара")] Product = 2,
		[Description("Наименованию товара и производителя")] ProductAndProducer = 3
	}

	[ActiveRecord("smart_order_rules", Schema = "ordersendrules", Lazy = true), Auditable]
	public class SmartOrderRules : IMultiAuditable
	{
		private string _columnSeparator;

		public SmartOrderRules()
		{
			ParseAlgorithm = "TestSource";
			Loader = LoaderType.Product;
		}

		[PrimaryKey("Id")]
		public virtual uint Id { get; set; }

		[Property, Description("Сопоставление"), Auditable]
		public virtual LoaderType Loader { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Description("Разделитель"), Auditable]
		public virtual string ColumnSeparator
		{
			get
			{
				if (_columnSeparator == "\t")
					return @"\t";
				return _columnSeparator;
			}
			set
			{
				if (value == @"\t")
					value = "\t";
				_columnSeparator = value;
			}
		}

		[Property, Description("Колонка код"), Auditable]
		public virtual string CodeColumn { get; set; }

		[Property, Description("Колонка код производителя"), Auditable]
		public virtual string CodeCrColumn { get; set; }

		[Property, Description("Колонка наименование товара"), Auditable]
		public virtual string ProductColumn { get; set; }

		[Property, Description("Колонка наименование производителя"), Auditable]
		public virtual string ProducerColumn { get; set; }

		[Property, Description("Колонка количество"), Auditable]
		public virtual string QuantityColumn { get; set; }

		[Property, Description("Код доставки"), Auditable]
		public virtual string SupplierDeliveryIdColumn { get; set; }

		[Property, Description("Служебные поля"), Auditable]
		public virtual string ServiceFields { get; set; }

		[Property, Description("Стартовая строка"), Auditable]
		public virtual int? StartLine { get; set; }

		[Property, Description("Кодовая станица"), Auditable]
		public virtual int? CodePage { get; set; }

		[Property, Description("Парсер для автозаказа"), Auditable]
		public virtual string ParseAlgorithm { get; set; }

		[BelongsTo, Description("Ассортиментный прайс лист для автозаказа"), Auditable]
		public virtual Price AssortimentPriceCode { get; set; }

		[HasMany]
		public virtual IList<DrugstoreSettings> Settings { get; set; }

		public static Encoding[] PosibleEncodings
		{
			get
			{
				return new[] {
					Encoding.GetEncoding(866),
					Encoding.GetEncoding(1251),
					Encoding.UTF8,
				};
			}
		}

		public virtual IEnumerable<IAuditRecord> GetAuditRecords(IEnumerable<AuditableProperty> properties = null)
		{
			return Settings.Where(s => s.Client != null).Select(s => new AuditRecord(s));
		}
	}
}