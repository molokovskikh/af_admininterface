using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AdminInterface.ManagerReportsFilters;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Queries
{
	public class ParsedWaybillsFilter : PaginableSortable, IFiltrable<ParsedWaybillsItem>
	{
		public DatePeriod Period { get; set; }

		public IList<ParsedWaybillsItem> Find()
		{
			var query = Session.CreateSQLQuery(@"select dh.firmcode as SupplierCode,
sp.Name as SupplierName,
r.Region as HomeRegion,
dh.Parser as Parser,
count(distinct dh.id) as WaybillsCount,
count(distinct ol.orderid) as OrdersCount,
round(sum(if(ol.RowId is null, 0, 1))/count(distinct db.id) * 100,2) as BindingPercent
from (documents.documentheaders dh,
documents.documentbodies db,
customers.suppliers sp,
farm.Regions r)
left join documents.waybillorders wo on wo.DocumentLineId=db.id
left join orders.orderslist ol on ol.RowId=wo.OrderLineId

where Date(dh.writetime)>=:begin and Date(dh.writetime)<=:end
and db.DocumentId=dh.id and dh.firmcode=sp.id and sp.HomeRegion=r.RegionCode
group by dh.firmcode, dh.Parser;")
				.SetParameter("begin", Period.Begin)
				.SetParameter("end", Period.End);
			var headers = query.ToList<ParsedWaybillsHeader>();
			headers = ApplySort(headers);
			var pagedHeaders = headers.Skip(PageSize * CurrentPage).Take(PageSize);
			//foreach (var parsedWaybillsHeader in pagedHeaders) {
			//	var a = GetItems(parsedWaybillsHeader.SupplierCode, parsedWaybillsHeader.Parser);
			//}
			var a = GetItems(headers);
			return a.ToList();
			return null;
			//Session.Query<Document>().
		}

		//private IList<ParsedWaybillsItem> GetItems(uint supplierCode, string parser)
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
from documents.documentheaders dh
join documents.documentbodies db
join documents.invoiceheaders ih
where Date(dh.writetime)>=:begin and Date(dh.writetime)<=:end
and dh.firmcode=:firmCode and dh.parser=:parser
and db.DocumentId=dh.id
and dh.id=ih.id
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
					.Replace("NdsAmount", "INdsAmount"));
			foreach (var parsedWaybillsHeader in headers) {
				var query = Session.CreateSQLQuery(queryString)
					.SetParameter("firmCode", parsedWaybillsHeader.SupplierCode)
					.SetParameter("parser", parsedWaybillsHeader.Parser)
					.SetParameter("begin", Period.Begin)
					.SetParameter("end", Period.End);
				var result = query.ToList<ParsedWaybillsItem>();
				if(result.Count > 0)
					yield return result[0];
			}
		}

		private IList<ParsedWaybillsHeader> ApplySort(IList<ParsedWaybillsHeader> headers)
		{
			return headers;
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

		[Display(Name = "Краткое наименование поставщика", Order = 0)]
		public string SupplierName { get; set; }

		[Display(Name = "Домашний регион поставщика", Order = 0)]
		public string HomeRegion { get; set; }

		[Display(Name = "Парсер", Order = 0)]
		public string Parser { get; set; }

		[Display(Name = "Кол-во накладных", Order = 0)]
		public int WaybillsCount { get; set; }

		[Display(Name = "Кол-во заявок, на которые ссылаются накладные", Order = 0)]
		public int OrdersCount { get; set; }

		[Display(Name = "Процент связывания позиций накладной с позициями заказа", Order = 0)]
		public double BindingPercent { get; set; }

		[Display(Name = "Наименование товара", Order = 0, GroupName = "CenterClass")]
		public string Product { get; set; }

		[Display(Name = "Код товара", Order = 0, GroupName = "CenterClass")]
		public string Code { get; set; }

		[Display(Name = "Сертификат", Order = 0, GroupName = "CenterClass")]
		public string Certificates { get; set; }

		[Display(Name = "Дата сертификата", Order = 0, GroupName = "CenterClass")]
		public string CertificatesDate { get; set; }

		[Display(Name = "Срок годности", Order = 0, GroupName = "CenterClass")]
		public string Period { get; set; }

		[Display(Name = "Производитель", Order = 0, GroupName = "CenterClass")]
		public string Producer { get; set; }

		[Display(Name = "Страна производителя", Order = 0, GroupName = "CenterClass")]
		public string Country { get; set; }

		[Display(Name = "Цена производителя", Order = 0, GroupName = "CenterClass")]
		public string ProducerCost { get; set; }

		[Display(Name = "Цена государственного реестра", Order = 0, GroupName = "CenterClass")]
		public string RegistryCost { get; set; }

		[Display(Name = "Наценка поставщика", Order = 0, GroupName = "CenterClass")]
		public string SupplierPriceMarkup { get; set; }

		[Display(Name = "Цена поставщика без НДС", Order = 0, GroupName = "CenterClass")]
		public string SupplierCostWithoutNDS { get; set; }

		[Display(Name = "Цена поставщика", Order = 0, GroupName = "CenterClass")]
		public string SupplierCost { get; set; }

		[Display(Name = "Количество", Order = 0, GroupName = "CenterClass")]
		public string Quantity { get; set; }

		[Display(Name = "Признак ЖНВЛС", Order = 0, GroupName = "CenterClass")]
		public string VitallyImportant { get; set; }

		[Display(Name = "Ставка НДС", Order = 0, GroupName = "CenterClass")]
		public string NDS { get; set; }

		[Display(Name = "Серийный номер", Order = 0, GroupName = "CenterClass")]
		public string SerialNumber { get; set; }

		[Display(Name = "Сумма с НДС", Order = 0, GroupName = "CenterClass")]
		public string Amount { get; set; }

		[Display(Name = "Сумма НДС", Order = 0, GroupName = "CenterClass")]
		public string NdsAmount { get; set; }

		[Display(Name = "Единица измерения", Order = 0, GroupName = "CenterClass")]
		public string Unit { get; set; }

		[Display(Name = "В том числе акциз", Order = 0, GroupName = "CenterClass")]
		public string ExciseTax { get; set; }

		[Display(Name = "№ Таможенной декларации", Order = 0, GroupName = "CenterClass")]
		public string BillOfEntryNumber { get; set; }

		[Display(Name = "Штрих-код", Order = 0, GroupName = "CenterClass")]
		public string EAN13 { get; set; }

		[Display(Name = "Код ОКДП", Order = 0, GroupName = "CenterClass")]
		public string CodeOKDP { get; set; }

		[Display(Name = "Имя файла образа сертификата", Order = 0, GroupName = "CenterClass")]
		public string CertificateFilename { get; set; }

		[Display(Name = "Имя файла образа протокола", Order = 0, GroupName = "CenterClass")]
		public string ProtocolFilemame { get; set; }

		[Display(Name = "Имя файла образа паспорта", Order = 0, GroupName = "CenterClass")]
		public string PassportFilename { get; set; }

		[Display(Name = "Орган, выдавший документа качества", Order = 0, GroupName = "CenterClass")]
		public string CertificateAuthority { get; set; }

		[Display(Name = "Срок годности в месяцах", Order = 0, GroupName = "CenterClass")]
		public string ExpireInMonths { get; set; }

		[Display(Name = "Дата изготовления", Order = 0, GroupName = "CenterClass")]
		public string DateOfManufacture { get; set; }

		[Display(Name = "Дата регистрации цены в ГосРеестре", Order = 0, GroupName = "CenterClass")]
		public string RegistryDate { get; set; }

		[Display(Name = "Код страны", Order = 0, GroupName = "CenterClass")]
		public string CountryCode { get; set; }

		[Display(Name = "Оптовая цена", Order = 0, GroupName = "CenterClass")]
		public string TradeCost { get; set; }

		[Display(Name = "Отпускная цена", Order = 0, GroupName = "CenterClass")]
		public string SaleCost { get; set; }

		[Display(Name = "Розничная цена", Order = 0, GroupName = "CenterClass")]
		public string RetailCost { get; set; }

		[Display(Name = "Шифр", Order = 0, GroupName = "CenterClass")]
		public string Cipher { get; set; }

		[Display(Name = "Номер счет-фактуры", Order = 0, GroupName = "CenterClass")]
		public string InvoiceNumber { get; set; }

		[Display(Name = "Дата счет-фактуры", Order = 0, GroupName = "CenterClass")]
		public string InvoiceDate { get; set; }

		[Display(Name = "Наименование продавца", Order = 0, GroupName = "CenterClass")]
		public string SellerName { get; set; }

		[Display(Name = "Адрес продавца", Order = 0, GroupName = "CenterClass")]
		public string SellerAddress { get; set; }

		[Display(Name = "ИНН продавца", Order = 0, GroupName = "CenterClass")]
		public string SellerINN { get; set; }

		[Display(Name = "КПП продавца", Order = 0, GroupName = "CenterClass")]
		public string SellerKPP { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string ShipperInfo { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string ConsigneeInfo { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string PaymentDocumentInfo { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string BuyerName { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string BuyerAddress { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string BuyerINN { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string BuyerKPP { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmountWithoutNDS0 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmountWithoutNDS10 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string NDSIAmount10 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmount10 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmountWithoutNDS18 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string NDSIAmount18 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmount18 { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmountWithoutNDS { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string NDSIAmount { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string IAmount { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string RecipientName { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string RecipientId { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string BuyerId { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string DelayOfPaymentInDays { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string DelayOfPaymentInBankDays { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string CommissionFeeContractId { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string CommissionFee { get; set; }

		[Display(Name = "", Order = 0, GroupName = "CenterClass")]
		public string StoreName { get; set; }
	}

	public static class ParsedWaybillsQueryHelper
	{
		public static string GetSelectMaxEachDocumentField(string alias)
		{
			var result = String.Join(",", DocumentBodyFields.Select(s => String.Format("Max({0}.{1})", alias, s)));
			return result;
		}

		public static string GetSelectMaxEachInvoiceField(string alias)
		{
			var result = String.Join(",", InvoiceHeadersFields.Select(s => String.Format("Max({0}.{1})", alias, s)));
			return result;
		}

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