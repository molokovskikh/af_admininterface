using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Queries
{
	public class ParsedWaybillsFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public DatePeriod Period { get; set; }
		public uint? ClientId { get; set; }
		public string ClientName { get; set; }

		public ParsedWaybillsFilter()
		{
			Period = new DatePeriod(DateTime.Today.AddDays(-1), DateTime.Today);
			PageSize = 30;
			SortKeyMap = new Dictionary<string, string> {
				{ "SupplierCode", "SupplierCode" },
				{ "SupplierName", "SupplierName" },
				{ "HomeRegion", "HomeRegion" },
				{ "Parser", "Parser" },
				{ "WaybillsCount", "WaybillsCount" },
				{ "OrdersCount", "OrdersCount" },
				{ "BindingPercent", "BindingPercent" }
			};
		}

		private IList<ParsedWaybillsHeader> ApplySort(IList<ParsedWaybillsHeader> items)
		{
			if(String.IsNullOrEmpty(SortBy)) {
				return items;
			}
			var propertyInfo = typeof(ParsedWaybillsHeader).GetProperty(SortBy);

			if(SortDirection.Contains("asc"))
				items = items.OrderBy(r => propertyInfo.GetValue(r, null)).ToList();
			else
				items = items.OrderByDescending(r => propertyInfo.GetValue(r, null)).ToList();
			return items;
		}

		public IList<BaseItemForTable> Find()
		{
			if(ClientId != null) {
				var client = Session.Load<Client>(ClientId);
				if(String.IsNullOrEmpty(ClientName) || !client.Name.Contains(ClientName)) {
					ClientId = null;
				}
			}
			var query = Session.CreateSQLQuery(String.Format(@"select dh.firmcode as SupplierCode,
sp.Name as SupplierName,
r.Region as HomeRegion,
dh.Parser as Parser,
count(distinct dh.id) as WaybillsCount,
count(distinct dh.orderid) as OrdersCount,
round(count(distinct wo.DocumentLineId)/count(distinct db.id) * 100,2) as BindingPercent
from (documents.documentheaders dh,
documents.documentbodies db,
customers.suppliers sp,
farm.Regions r)
{0}
left join documents.waybillorders wo on wo.DocumentLineId=db.id

where Date(dh.writetime)>=:begin and Date(dh.writetime)<=:end
and db.DocumentId=dh.id and dh.firmcode=sp.id and sp.HomeRegion=r.RegionCode
group by dh.firmcode, dh.Parser
order by sp.Name, r.Region, dh.Parser;",
				ClientId == null || String.IsNullOrEmpty(ClientName) ? "" : String.Format(
					"join logs.document_logs dl on dl.rowid=dh.downloadid and dl.ClientCode={0}",
					ClientId.Value)))
				.SetParameter("begin", Period.Begin)
				.SetParameter("end", Period.End);
			var headers = query.ToList<ParsedWaybillsHeader>();
			headers = ApplySort(headers);
			RowsCount = headers.Count;
			headers = ApplySort(headers);
			var pagedHeaders = headers.Skip(PageSize * CurrentPage).Take(PageSize);
			var items = GetItems(pagedHeaders);
			return items.Cast<BaseItemForTable>().ToList();
		}

		private IEnumerable<ParsedWaybillsItem> GetItems(IEnumerable<ParsedWaybillsHeader> headers)
		{
			var queryString = String.Format(@"DROP TEMPORARY TABLE IF EXISTS usersettings.WaybillParsedInfo;
CREATE temporary table usersettings.WaybillParsedInfo(
 SupplierCode int(11) unsigned NOT NULL,
 Parser varchar(255) DEFAULT NULL,
{0},
{1}
 ) engine=MEMORY;

insert into usersettings.WaybillParsedInfo
 select dh.firmcode, dh.Parser,
{2},
{3}
from (documents.documentheaders dh,
documents.documentbodies db)
{6}
left join documents.invoiceheaders ih on dh.id=ih.id
where Date(dh.writetime)>=:begin and Date(dh.writetime)<=:end
and dh.firmcode=:firmCode and (dh.parser=:parser or (dh.parser is null and :parser is null))
and db.DocumentId=dh.id
limit 50;

select info.SupplierCode, info.Parser, {4}, {5}
from usersettings.WaybillParsedInfo info
group by info.SupplierCode, info.Parser;",
				ParsedWaybillsQueryHelper.GetSelectWithFormat(
					ParsedWaybillsQueryHelper.DocumentBodyFields, "\n{0} varchar(1) DEFAULT NULL"),
				ParsedWaybillsQueryHelper.GetSelectWithFormat(
					ParsedWaybillsQueryHelper.InvoiceHeadersFields, "\n{0} varchar(1) DEFAULT NULL")
					.Replace("Amount", "IAmount")
					.Replace("NdsAmount", "INdsAmount"),
				ParsedWaybillsQueryHelper.GetSelectWithFormat(
					ParsedWaybillsQueryHelper.DocumentBodyFields, "if(db.{0} is null, null, '*')"),
				ParsedWaybillsQueryHelper.GetSelectWithFormat(
					ParsedWaybillsQueryHelper.InvoiceHeadersFields, "if(ih.{0} is null, null, '*')"),
				ParsedWaybillsQueryHelper.GetSelectWithFormat(
					ParsedWaybillsQueryHelper.DocumentBodyFields, "if(Max(info.{0}) is null, null, '*') as {0}"),
				ParsedWaybillsQueryHelper.GetSelectWithFormat(
					ParsedWaybillsQueryHelper.InvoiceHeadersFields, "if(Max(info.{0}) is null, null, '*') as {0}")
					.Replace("Amount", "IAmount")
					.Replace("NdsAmount", "INdsAmount"),
				ClientId == null || String.IsNullOrEmpty(ClientName) ? "" : String.Format(
					"join logs.document_logs dl on dl.rowid=dh.downloadid and dl.ClientCode={0}",
					ClientId.Value));
			foreach (var parsedWaybillsHeader in headers) {
				var query = Session.CreateSQLQuery(queryString)
					.SetParameter("firmCode", parsedWaybillsHeader.SupplierCode)
					.SetParameter("parser", parsedWaybillsHeader.Parser)
					.SetParameter("begin", Period.Begin)
					.SetParameter("end", Period.End);
				var result = query.ToList<ParsedWaybillsItem>();
				if(result.Count > 0) {
					result[0].SupplierName = parsedWaybillsHeader.SupplierName;
					result[0].HomeRegion = parsedWaybillsHeader.HomeRegion;
					result[0].WaybillsCount = parsedWaybillsHeader.WaybillsCount;
					result[0].OrdersCount = parsedWaybillsHeader.OrdersCount;
					result[0].BindingPercent = parsedWaybillsHeader.BindingPercent;
					yield return result[0];
				}
			}
		}

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }
	}

	public class ParsedWaybillsHeader
	{
		public uint SupplierCode { get; set; }
		public string SupplierName { get; set; }
		public string HomeRegion { get; set; }
		public string Parser { get; set; }
		public int WaybillsCount { get; set; }
		public int OrdersCount { get; set; }
		public double BindingPercent { get; set; }
	}

	public class ParsedWaybillsItem : BaseItemForTable
	{
		[Display(Name = "Код поставщика", Order = 0)]
		public uint SupplierCode { get; set; }

		[Display(Name = "Краткое наименование поставщика", Order = 1)]
		public string SupplierName { get; set; }

		[Display(Name = "Домашний регион поставщика", Order = 2)]
		public string HomeRegion { get; set; }

		[Display(Name = "Парсер", Order = 3)]
		public string Parser { get; set; }

		[Display(Name = "Кол-во накладных", Order = 4)]
		public int WaybillsCount { get; set; }

		[Display(Name = "Кол-во заявок, на которые ссылаются накладные", Order = 5)]
		public int OrdersCount { get; set; }

		[Display(Name = "Процент связывания позиций накладной с позициями заказа", Order = 6)]
		public double BindingPercent { get; set; }

		[Display(Name = "Наименование товара", Order = 10, GroupName = "CenterClass")]
		public string Product { get; set; }

		[Display(Name = "Код товара", Order = 10, GroupName = "CenterClass")]
		public string Code { get; set; }

		[Display(Name = "Сертификат", Order = 10, GroupName = "CenterClass")]
		public string Certificates { get; set; }

		[Display(Name = "Дата сертификата", Order = 10, GroupName = "CenterClass")]
		public string CertificatesDate { get; set; }

		[Display(Name = "Срок годности", Order = 10, GroupName = "CenterClass")]
		public string Period { get; set; }

		[Display(Name = "Производитель", Order = 10, GroupName = "CenterClass")]
		public string Producer { get; set; }

		[Display(Name = "Страна производителя", Order = 10, GroupName = "CenterClass")]
		public string Country { get; set; }

		[Display(Name = "Цена производителя", Order = 10, GroupName = "CenterClass")]
		public string ProducerCost { get; set; }

		[Display(Name = "Цена государственного реестра", Order = 10, GroupName = "CenterClass")]
		public string RegistryCost { get; set; }

		[Display(Name = "Наценка поставщика", Order = 10, GroupName = "CenterClass")]
		public string SupplierPriceMarkup { get; set; }

		[Display(Name = "Цена поставщика без НДС", Order = 10, GroupName = "CenterClass")]
		public string SupplierCostWithoutNDS { get; set; }

		[Display(Name = "Цена поставщика", Order = 10, GroupName = "CenterClass")]
		public string SupplierCost { get; set; }

		[Display(Name = "Количество", Order = 10, GroupName = "CenterClass")]
		public string Quantity { get; set; }

		[Display(Name = "Признак ЖНВЛС", Order = 10, GroupName = "CenterClass")]
		public string VitallyImportant { get; set; }

		[Display(Name = "Ставка НДС", Order = 10, GroupName = "CenterClass")]
		public string NDS { get; set; }

		[Display(Name = "Серийный номер", Order = 10, GroupName = "CenterClass")]
		public string SerialNumber { get; set; }

		[Display(Name = "Сумма с НДС", Order = 10, GroupName = "CenterClass")]
		public string Amount { get; set; }

		[Display(Name = "Сумма НДС", Order = 10, GroupName = "CenterClass")]
		public string NdsAmount { get; set; }

		[Display(Name = "Единица измерения", Order = 10, GroupName = "CenterClass")]
		public string Unit { get; set; }

		[Display(Name = "В том числе акциз", Order = 10, GroupName = "CenterClass")]
		public string ExciseTax { get; set; }

		[Display(Name = "№ Таможенной декларации", Order = 10, GroupName = "CenterClass")]
		public string BillOfEntryNumber { get; set; }

		[Display(Name = "Штрих-код", Order = 10, GroupName = "CenterClass")]
		public string EAN13 { get; set; }

		[Display(Name = "Код ОКДП", Order = 10, GroupName = "CenterClass")]
		public string CodeOKDP { get; set; }

		[Display(Name = "Имя файла образа сертификата", Order = 10, GroupName = "CenterClass")]
		public string CertificateFilename { get; set; }

		[Display(Name = "Имя файла образа протокола", Order = 10, GroupName = "CenterClass")]
		public string ProtocolFilemame { get; set; }

		[Display(Name = "Имя файла образа паспорта", Order = 10, GroupName = "CenterClass")]
		public string PassportFilename { get; set; }

		[Display(Name = "Орган, выдавший документа качества", Order = 10, GroupName = "CenterClass")]
		public string CertificateAuthority { get; set; }

		[Display(Name = "Срок годности в месяцах", Order = 10, GroupName = "CenterClass")]
		public string ExpireInMonths { get; set; }

		[Display(Name = "Дата изготовления", Order = 10, GroupName = "CenterClass")]
		public string DateOfManufacture { get; set; }

		[Display(Name = "Дата регистрации цены в ГосРеестре", Order = 10, GroupName = "CenterClass")]
		public string RegistryDate { get; set; }

		[Display(Name = "Код страны", Order = 10, GroupName = "CenterClass")]
		public string CountryCode { get; set; }

		[Display(Name = "Оптовая цена", Order = 10, GroupName = "CenterClass")]
		public string TradeCost { get; set; }

		[Display(Name = "Отпускная цена", Order = 10, GroupName = "CenterClass")]
		public string SaleCost { get; set; }

		[Display(Name = "Розничная цена", Order = 10, GroupName = "CenterClass")]
		public string RetailCost { get; set; }

		[Display(Name = "Шифр", Order = 10, GroupName = "CenterClass")]
		public string Cipher { get; set; }

		[Display(Name = "Номер счет-фактуры", Order = 10, GroupName = "CenterClass")]
		public string InvoiceNumber { get; set; }

		[Display(Name = "Дата счет-фактуры", Order = 10, GroupName = "CenterClass")]
		public string InvoiceDate { get; set; }

		[Display(Name = "Наименование продавца", Order = 10, GroupName = "CenterClass")]
		public string SellerName { get; set; }

		[Display(Name = "Адрес продавца", Order = 10, GroupName = "CenterClass")]
		public string SellerAddress { get; set; }

		[Display(Name = "ИНН продавца", Order = 10, GroupName = "CenterClass")]
		public string SellerINN { get; set; }

		[Display(Name = "КПП продавца", Order = 10, GroupName = "CenterClass")]
		public string SellerKPP { get; set; }

		[Display(Name = "Грузоотправитель и его адрес", Order = 10, GroupName = "CenterClass")]
		public string ShipperInfo { get; set; }

		[Display(Name = "Грузополучатель и его адрес", Order = 10, GroupName = "CenterClass")]
		public string ConsigneeInfo { get; set; }

		[Display(Name = "Поле К платежно-расчетному документу N", Order = 10, GroupName = "CenterClass")]
		public string PaymentDocumentInfo { get; set; }

		[Display(Name = "Наименование покупателя", Order = 10, GroupName = "CenterClass")]
		public string BuyerName { get; set; }

		[Display(Name = "Адрес покупателя", Order = 10, GroupName = "CenterClass")]
		public string BuyerAddress { get; set; }

		[Display(Name = "ИНН покупателя", Order = 10, GroupName = "CenterClass")]
		public string BuyerINN { get; set; }

		[Display(Name = "КПП покупателя", Order = 10, GroupName = "CenterClass")]
		public string BuyerKPP { get; set; }

		[Display(Name = "Стоимость товаров для группы товаров, облагаемых ставкой 0% НДС без учета НДС", Order = 10, GroupName = "CenterClass")]
		public string IAmountWithoutNDS0 { get; set; }

		[Display(Name = "Стоимость товаров для группы товаров, облагаемых ставкой 10% НДС без учета НДС", Order = 10, GroupName = "CenterClass")]
		public string IAmountWithoutNDS10 { get; set; }

		[Display(Name = "Сумма налога для группы товаров, облагаемых ставкой 10% НДС", Order = 10, GroupName = "CenterClass")]
		public string NDSIAmount10 { get; set; }

		[Display(Name = "Стоимость товаров для группы товаров, облагаемых ставкой 10% НДС", Order = 10, GroupName = "CenterClass")]
		public string IAmount10 { get; set; }

		[Display(Name = "Стоимость товаров для группы товаров, облагаемых ставкой 18% НДС без учета НДС", Order = 10, GroupName = "CenterClass")]
		public string IAmountWithoutNDS18 { get; set; }

		[Display(Name = "Сумма налога для группы товаров, облагаемых ставкой 18% НДС", Order = 10, GroupName = "CenterClass")]
		public string NDSIAmount18 { get; set; }

		[Display(Name = "Стоимость товаров для группы товаров, облагаемых ставкой 18% НДС", Order = 10, GroupName = "CenterClass")]
		public string IAmount18 { get; set; }

		[Display(Name = "Общая стоимость товаров без учета НДС", Order = 10, GroupName = "CenterClass")]
		public string IAmountWithoutNDS { get; set; }

		[Display(Name = "Общая сумма налога", Order = 10, GroupName = "CenterClass")]
		public string NDSIAmount { get; set; }

		[Display(Name = "Общая стоимость товаров", Order = 10, GroupName = "CenterClass")]
		public string IAmount { get; set; }

		[Display(Name = "Название грузополучателя", Order = 10, GroupName = "CenterClass")]
		public string RecipientName { get; set; }

		[Display(Name = "Код грузополучателя", Order = 10, GroupName = "CenterClass")]
		public string RecipientId { get; set; }

		[Display(Name = "Код покупателя", Order = 10, GroupName = "CenterClass")]
		public string BuyerId { get; set; }

		[Display(Name = "Отсрочка платежа (календарные дни)", Order = 10, GroupName = "CenterClass")]
		public string DelayOfPaymentInDays { get; set; }

		[Display(Name = "Отсрочка платежа (банковские дни)", Order = 10, GroupName = "CenterClass")]
		public string DelayOfPaymentInBankDays { get; set; }

		[Display(Name = "Номер договора (комиссии)", Order = 10, GroupName = "CenterClass")]
		public string CommissionFeeContractId { get; set; }

		[Display(Name = "Ставка комиссионного вознаграждения", Order = 10, GroupName = "CenterClass")]
		public string CommissionFee { get; set; }

		[Display(Name = "Склад", Order = 10, GroupName = "CenterClass")]
		public string StoreName { get; set; }
	}

	public static class ParsedWaybillsQueryHelper
	{
		public static string GetSelectWithFormat(string[] source, string format, string separator = ",")
		{
			var result = String.Join(separator, source.Select(s => String.Format(format, s)));
			return result;
		}

		private static string[] _documentBodyFields = new string[] {
			"Product",
			"Code",
			"Certificates",
			"CertificatesDate",
			"Period",
			"Producer",
			"Country",
			"ProducerCost",
			"RegistryCost",
			"SupplierPriceMarkup",
			"SupplierCostWithoutNDS",
			"SupplierCost",
			"Quantity",
			"VitallyImportant",
			"NDS",
			"SerialNumber",
			"Amount",
			"NdsAmount",
			"Unit",
			"ExciseTax",
			"BillOfEntryNumber",
			"EAN13",
			"CodeOKDP",
			"CertificateFilename",
			"ProtocolFilemame",
			"PassportFilename",
			"CertificateAuthority",
			"ExpireInMonths",
			"DateOfManufacture",
			"RegistryDate",
			"CountryCode",
			"TradeCost",
			"SaleCost",
			"RetailCost",
			"Cipher"
		};

		private static string[] _invoiceHeadersFields = new string[] {
			"InvoiceNumber",
			"InvoiceDate",
			"SellerName",
			"SellerAddress",
			"SellerINN",
			"SellerKPP",
			"ShipperInfo",
			"ConsigneeInfo",
			"PaymentDocumentInfo",
			"BuyerName",
			"BuyerAddress",
			"BuyerINN",
			"BuyerKPP",
			"AmountWithoutNDS0",
			"AmountWithoutNDS10",
			"NDSAmount10",
			"Amount10",
			"AmountWithoutNDS18",
			"NDSAmount18",
			"Amount18",
			"AmountWithoutNDS",
			"NDSAmount",
			"Amount",
			"RecipientName",
			"RecipientId",
			"BuyerId",
			"DelayOfPaymentInDays",
			"DelayOfPaymentInBankDays",
			"CommissionFeeContractId",
			"CommissionFee",
			"StoreName"
		};

		public static string[] DocumentBodyFields
		{
			get { return _documentBodyFields; }
			set { _documentBodyFields = value; }
		}

		public static string[] InvoiceHeadersFields
		{
			get { return _invoiceHeadersFields; }
			set { _invoiceHeadersFields = value; }
		}
	}
}