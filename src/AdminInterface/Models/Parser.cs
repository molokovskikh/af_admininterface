using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;

namespace AdminInterface.Models
{
	public enum EncodingEnum
	{
		Default,
		UTF8,
		UTF7,
		CP1251,
		CP866
	}

	[ActiveRecord(Schema = "Customers")]
	public class Parser
	{
		public Parser(Supplier supplier, EncodingEnum encoding) : 
			this(supplier)
		{
			Encoding = encoding;
		}

		public Parser(Supplier supplier)
			: this()
		{
			Supplier = supplier;
		}

		public Parser()
		{
			Lines = new List<ParserLine>();
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, DisplayName("Название"), Required]
		public virtual string Name { get; set; }

		[Property, DisplayName("Кодировка"), Required]
		public virtual EncodingEnum Encoding { get; set; }


		[BelongsTo]
		public virtual Supplier Supplier { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public virtual IList<ParserLine> Lines { get; set; }

		public List<SelectListItem> Fields(string selected)
		{
			var headerGroup = new SelectListGroup {
				Name = "Заголовок"
			};
			var lineGroup = new SelectListGroup {
				Name = "Строка"
			};
			var items = new List<SelectListItem> {

				//шапка
				new SelectListItem {
					Text = "Номер накладной",
					Value = "Header_ProviderDocumentId",
					Group = headerGroup
				},
				new SelectListItem {
					Text = "Дата накладной",
					Value = "Header_DocumentDate",
					Group = headerGroup
				},

				//накладная
				new SelectListItem {
					Text = "Номер счет-фактуры",
					Value = "Invoice_InvoiceNumber",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Дата счет-фактуры",
					Value = "Invoice_InvoiceDate",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Наименование продавца",
					Value = "Invoice_SellerName",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Адрес продавца",
					Value = "Invoice_SellerAddress",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "ИНН продавца",
					Value = "Invoice_SellerINN",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "КПП продавца",
					Value = "Invoice_SellerKPP",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Грузоотправитель и его адрес",
					Value = "Invoice_ShipperInfo",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Название грузополучателя",
					Value = "Invoice_RecipientName",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Код грузополучателя в кодировке поставщика",
					Value = "Invoice_RecipientId",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Грузополучатель и его адрес",
					Value = "Invoice_RecipientAddress",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Поле К платежно-расчетному документу N",
					Value = "Invoice_PaymentDocumentInfo",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Код покупателя в кодировки поставщика",
					Value = "Invoice_BuyerId",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Наименование покупателя",
					Value = "Invoice_BuyerName",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Адрес покупателя",
					Value = "Invoice_BuyerAddress",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "ИНН покупателя",
					Value = "Invoice_BuyerINN",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "КПП покупателя",
					Value = "Invoice_BuyerKPP",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Стоимость товаров для группы товаров, облагаемых ставкой 0% НДС",
					Value = "Invoice_AmountWithoutNDS0",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Стоимость товаров без налога для группы товаров, облагаемых ставкой 10% НДС",
					Value = "Invoice_AmountWithoutNDS10",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Сумма налога для группы товаров, облагаемых ставкой 10% НДС",
					Value = "Invoice_NDSAmount10",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Стоимость товаров для группы товаров, облагаемых ставкой 10% НДС всего с учётом налога",
					Value = "Invoice_Amount10",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Стоимость товаров без налога для группы товаров, облагаемых ставкой 18% НДС",
					Value = "Invoice_AmountWithoutNDS18",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Сумма налога для группы товаров, облагаемых ставкой 18% НДС",
					Value = "Invoice_NDSAmount18",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Стоимость товаров для группы товаров , облагаемых ставкой 18% НДС всего с учётом налога",
					Value = "Invoice_Amount18",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Общая стоимость товаров без налога (указывается в конце таблицы счёт-фактуры по строке «ИТОГО»)",
					Value = "Invoice_AmountWithoutNDS",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Общая сумма налога (указывается в конце таблицы счёт-фактуры по строке «ИТОГО»)",
					Value = "Invoice_NDSAmount",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Общая стоимость товаров с налогом (указывается в конце таблицы счёт-фактуры по строке «ИТОГО»)",
					Value = "Invoice_Amount",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Отсрочка платежа (календарные дни)",
					Value = "Invoice_DelayOfPaymentInDays",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Отсрочка платежа (банковские дни)",
					Value = "Invoice_DelayOfPaymentInBankDays",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Номер договора (комиссии)",
					Value = "Invoice_CommissionFeeContractId",
					Group = headerGroup,
				},
				new SelectListItem {
					Text = "Ставка комиссионного вознаграждения",
					Value = "Invoice_CommissionFee",
					Group = headerGroup,
				},

				//строка
				new SelectListItem {
					Text = "Наименование продукта",
					Value = "Product",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Код товара поставщика",
					Value = "Code",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Информация о сертификате",
					Value = "Certificates",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Дата выдачи сертификата",
					Value = "CertificatesDate",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Срок действия сертификата, дата окончания",
					Value = "CertificatesEndDate",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Орган, выдавший документа качества",
					Value = "CertificateAuthority",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Срок годности. А точнее Дата окончания срока годности",
					Value = "Period",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Срок годности в месяцах",
					Value = "ExpireInMonths",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Дата изготовления",
					Value = "DateOfManufacture",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Производитель",
					Value = "Producer",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Страна производителя",
					Value = "Country",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Цена производителя без НДС",
					Value = "ProducerCostWithoutNDS",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Цена государственного реестра",
					Value = "RegistryCost",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Дата регистрации цены в ГосРеестре",
					Value = "RegistryDate",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Наценка поставщика",
					Value = "SupplierPriceMarkup",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Ставка налога на добавленную стоимость",
					Value = "Nds",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Цена поставщика без НДС",
					Value = "SupplierCostWithoutNDS",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Цена поставщика с НДС",
					Value = "SupplierCost",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Количество",
					Value = "Quantity",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Признак ЖНВЛС",
					Value = "VitallyImportant",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Серийный номер продукта",
					Value = "SerialNumber",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Сумма НДС",
					Value = "NdsAmount",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Сумма с НДС",
					Value = "Amount",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Единица измерения",
					Value = "Unit",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Код единицы измерения",
					Value = "UnitCode",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "В том числе акциз",
					Value = "ExciseTax",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "№ Таможенной декларации, Номер ГТД",
					Value = "BillOfEntryNumber",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Код EAN-13 (штрих-код)",
					Value = "EAN13",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Номер заказа",
					Value = "OrderId",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Код ОКДП",
					Value = "CodeOKDP",
					Group = lineGroup,
				},
			};
			items.Each(x => x.Selected = x.Value == selected);
			items = items.OrderBy(s => s.Group.Name).ThenBy(s => s.Text).ToList();
			return items;
		}

		public List<SelectListItem> Encodings ()
		{
			var items = new List<SelectListItem> {

				new SelectListItem {
					Text = "По умолчанию",
					Value = EncodingEnum.Default.ToString(),
				},

				new SelectListItem {
					Text = "UTF-8",
					Value = EncodingEnum.UTF8.ToString(),
				},

				new SelectListItem {
					Text = "UTF-7",
					Value = EncodingEnum.UTF7.ToString(),
				},

				new SelectListItem {
					Text = "CP1251",
					Value = EncodingEnum.CP1251.ToString(),
				},

				new SelectListItem {
					Text = "CP866",
					Value = EncodingEnum.CP866.ToString(),
				},

			};

			return items;
		}
	}

	[ActiveRecord(Schema = "Customers")]
	public class ParserLine
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo]
		public virtual Parser Parser { get; set; }

		[Property]
		public virtual string Src { get; set; }

		[Property]
		public virtual string Dst { get; set; }
	}
}