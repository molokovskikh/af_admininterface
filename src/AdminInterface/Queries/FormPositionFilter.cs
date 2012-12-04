using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AdminInterface.ManagerReportsFilters;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;

namespace AdminInterface.Queries
{
	public class FormPositionItem : BaseItemForTable
	{
		[Display(Name = "Код поставщика", Order = 3)]
		public uint SupplierCode { get; set; }
		[Display(Name = "Наименование поставщика", Order = 3)]
		public string SupplierName { get; set; }
		[Display(Name = "Домашний регион", Order = 3)]
		public string Region { get; set; }
		[Display(Name = "Код прайс-листа", Order = 3)]
		public uint PriceCode { get; set; }
		[Display(Name = "Наименование прайс-листа", Order = 3)]
		public string PriceName { get; set; }
		[Display(Name = "Установленная старость прайс-листа", Order = 3)]
		public int MaxOld { get; set; }
		[Display(Name = "Установленное ожидание прайс-листа", Order = 3)]
		public int WaitingDownloadInterval { get; set; }
		[Display(Name = "Количество ценовых колонок", Order = 3)]
		public int CostCount { get; set; }
		[Display(Name = "Код товара", Order = 3)]
		public string FCode { get; set; }
		[Display(Name = "Код производителя", Order = 3)]
		public string FCodeCr { get; set; }
		[Display(Name = "Наименование товара", Order = 3)]
		public string FName { get; set; }
		[Display(Name = "Наименование производителя", Order = 3)]
		public string FFirmCr { get; set; }
		[Display(Name = "Минимальная цена", Order = 3)]
		public string FMinBoundCost { get; set; }
		[Display(Name = "Единица измерения", Order = 3)]
		public string FUnit { get; set; }
		[Display(Name = "Цеховая упаковка", Order = 3)]
		public string FVolume { get; set; }
		[Display(Name = "Количество", Order = 3)]
		public string FQuantity { get; set; }
		[Display(Name = "Примечание", Order = 3)]
		public string FNote { get; set; }
		[Display(Name = "Срок годности", Order = 3)]
		public string FPeriod { get; set; }
		[Display(Name = "Документ", Order = 3)]
		public string FDoc { get; set; }
		[Display(Name = "Срок", Order = 3)]
		public string FJunk { get; set; }
		[Display(Name = "Ожидается", Order = 3)]
		public string FAwait { get; set; }
		[Display(Name = "Жизненно важный", Order = 3)]
		public string FVitallyImportant { get; set; }
		[Display(Name = "Кратность", Order = 3)]
		public string FRequestRatio { get; set; }
		[Display(Name = "Реестровая цена", Order = 3)]
		public string FRegistryCost { get; set; }
		[Display(Name = "Цена максимальная", Order = 3)]
		public string FMaxBoundCost { get; set; }
		[Display(Name = "Минимальная сумма", Order = 3)]
		public string FOrderCost { get; set; }
		[Display(Name = "Минимальное количество", Order = 3)]
		public string FMinOrderCount { get; set; }
		[Display(Name = "Цена производителя", Order = 3)]
		public string FProducerCost { get; set; }
		[Display(Name = "Ставка НДС", Order = 3)]
		public string FNds { get; set; }
		[Display(Name = "Код EAN-13 (штрих-код)", Order = 3)]
		public string FEAN13 { get; set; }
		[Display(Name = "Серия", Order = 3)]
		public string FSeries { get; set; }
		[Display(Name = "Код ОКП", Order = 3)]
		public string FCodeOKP { get; set; }
	}

	public class FormPositionFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public IList<BaseItemForTable> Find()
		{
			var result = Session.CreateSQLQuery(_queryString).ToList<FormPositionItem>();
			return result.Cast<BaseItemForTable>().ToList();
		}

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public FormPositionFilter()
		{
			SortBy = "SupplierName";
			SortKeyMap = new Dictionary<string, string> {
				{ "SupplierName", "SupplierName" },
				{ "Region", "Region" },
				{ "PriceName", "PriceName" }
			};
		}

		private string _queryString = @"drop temporary table IF EXISTS farm.TmpPricesCode;
CREATE temporary table farm.TmpPricesCode(
  PriceCode bigint unsigned
  ) engine=MEMORY;

INSERT INTO farm.TmpPricesCode
SELECT distinct ps.PriceCode FROM usersettings.priceitems p
join farm.formrules f on p.FormRuleId=f.Id
join usersettings.pricescosts ps on ps.PriceItemId = p.Id
join usersettings.pricesdata pd on ps.pricecode=pd.pricecode
join customers.suppliers s on pd.FirmCode=s.id
join farm.pricefmts pf on f.PriceFormatId=pf.id
where pf.format in ('XML', 'Sudakov', 'Формат для Фармаимпе', 'Универсальный Xml')
 and pd.Enabled=1
and pd.Agencyenabled=1
and s.Disabled=0;

DROP TABLE IF EXISTS `usersettings`.`corefields`;
CREATE temporary table  `usersettings`.`corefields` (
  `PriceCode` int(11) unsigned NOT NULL,
  `FCode` varchar(20) DEFAULT NULL,
  `FCodeCr` varchar(20) DEFAULT NULL,
  `FName` varchar(20) DEFAULT NULL,
   `FFirmCr` varchar(20) DEFAULT NULL,
  `FMinBoundCost` varchar(20) DEFAULT NULL,
  `FUnit` varchar(20) DEFAULT NULL,
  `FVolume` varchar(20) DEFAULT NULL,
  `FQuantity` varchar(20) DEFAULT NULL,
  `FNote` varchar(100) DEFAULT NULL,
  `FPeriod` varchar(20) DEFAULT NULL,
  `FDoc` varchar(20) DEFAULT NULL,
  `FJunk` varchar(20) DEFAULT NULL,
  `FAwait` varchar(20) DEFAULT NULL,
  `FVitallyImportant` varchar(20) DEFAULT NULL,
  `FRequestRatio` varchar(20) DEFAULT NULL,
  `FRegistryCost` varchar(20) DEFAULT NULL,
  `FMaxBoundCost` varchar(20) DEFAULT NULL,
  `FOrderCost` varchar(20) DEFAULT NULL,
  `FMinOrderCount` varchar(20) DEFAULT NULL,
  `FProducerCost` varchar(20) DEFAULT NULL,
  `FNds` varchar(20) DEFAULT NULL,
  `FEAN13` varchar(20) DEFAULT NULL,
  `FSeries` varchar(20) DEFAULT NULL,
  `FCodeOKP` varchar(20) DEFAULT NULL
) engine=MEMORY;

insert into `usersettings`.`corefields`
select pc.PriceCode, Max(c0.Code), Max(c0.CodeCr), Max(c0.SynonymCode), Max(c0.SynonymFirmCrCode),
Max(c0.MinBoundCost), Max(c0.Unit), Max(c0.Volume), Max(c0.Quantity), Max(c0.Note), Max(c0.Period), Max(c0.Doc), Max(c0.Junk), Max(c0.Await),
Max(c0.VitallyImportant), Max(c0.RequestRatio), Max(c0.RegistryCost), Max(c0.MaxBoundCost), Max(c0.OrderCost), Max(c0.MinOrderCount),
Max(c0.ProducerCost), Max(c0.Nds), Max(c0.EAN13), Max(c0.Series), Max(c0.CodeOKP)
from farm.TmpPricesCode pc join farm.Core0 c0 on c0.PriceCode=pc.PriceCode
group by pc.PriceCode;

insert into `usersettings`.`corefields`
SELECT distinct ps.PriceCode, FCode, FCodeCr,ifnull(ifnull(FName1,FName2),FName3),FFirmCr,FMinBoundCost,FUnit,FVolume,
FQuantity,FNote,FPeriod,FDoc,FJunk,FAwait,FVitallyImportant,FRequestRatio,FRegistryCost,FMaxBoundCost,
FOrderCost,FMinOrderCount,FProducerCost,FNds,FEAN13,FSeries,FCodeOKP
 FROM usersettings.priceitems p
join farm.formrules f on p.FormRuleId=f.Id
join usersettings.pricescosts ps on ps.PriceItemId = p.Id
join usersettings.pricesdata pd on ps.pricecode=pd.pricecode
join customers.suppliers s on pd.FirmCode=s.id
join farm.pricefmts pf on f.PriceFormatId=pf.id
where pf.format not in ('XML', 'Sudakov', 'Формат для Фармаимпе', 'Универсальный Xml')
 and pd.Enabled=1
and pd.Agencyenabled=1
and s.Disabled=0
order by ps.PriceCode;

DROP TABLE IF EXISTS `usersettings`.`pricesInfo`;
CREATE temporary table  `usersettings`.`pricesInfo` (
  `SupplierCode` int(11) unsigned NOT NULL,
  `SupplierName` varchar(255) DEFAULT NULL,
  `Region` varchar(30) DEFAULT NULL,
  `PriceCode` int(11) unsigned NOT NULL,
  `PriceName` varchar(50) DEFAULT NULL,
  `MaxOld` int(11) DEFAULT NULL,
  `WaitingDownloadInterval` int(11) DEFAULT 1,
  `CostCount` int(11) DEFAULT 0
  ) engine=MEMORY;
  insert into `usersettings`.`pricesInfo`
select distinct s.Id, s.Name, rg.Region, pd.PriceCode, pd.PriceName, f.MaxOld, p.WaitingDownloadInterval,
sum(if(ps.Enabled = 1 and ps.AgencyEnabled=1, 1, 0)) as CostCount
from usersettings.pricesdata pd
join usersettings.pricescosts ps on ps.pricecode=pd.pricecode
join usersettings.priceitems p on ps.PriceItemId = p.Id
join customers.suppliers s on pd.FirmCode=s.id
join farm.regions rg on s.homeregion = rg.RegionCode
join farm.formrules f on p.FormRuleId=f.Id
group by s.Id, s.Name, rg.Region, pd.PriceCode, pd.PriceName, f.MaxOld, p.WaitingDownloadInterval;

select distinct pd.*,
if(cf.FCode is null,null,'*') as FCode ,
if(cf.FCodeCr is null,null,'*') as FCodeCr ,
if(cf.FName is null,null,'*') as FName ,
if(cf.FFirmCr is null,null,'*') as FFirmCr ,
if(cf.FMinBoundCost is null,null,'*') as FMinBoundCost ,
if(cf.FUnit is null,null,'*') as FUnit ,
if(cf.FVolume is null,null,'*') as FVolume ,
if(cf.FQuantity is null,null,'*') as FQuantity ,
if(cf.FNote is null,null,'*') as FNote ,
if(cf.FPeriod is null,null,'*') as FPeriod ,
if(cf.FDoc is null,null,'*') as FDoc ,
if(cf.FJunk is null,null,'*') as FJunk ,
if(cf.FAwait is null,null,'*') as FAwait ,
if(cf.FVitallyImportant is null,null,'*') as FVitallyImportant ,
if(cf.FRequestRatio is null,null,'*') as FRequestRatio ,
if(cf.FRegistryCost is null,null,'*') as FRegistryCost ,
if(cf.FMaxBoundCost is null,null,'*') as FMaxBoundCost ,
if(cf.FOrderCost is null,null,'*') as FOrderCost ,
if(cf.FMinOrderCount is null,null,'*') as FMinOrderCount ,
if(cf.FProducerCost is null,null,'*') as FProducerCost ,
if(cf.FNds is null,null,'*') as FNds ,
if(cf.FEAN13 is null,null,'*') as FEAN13 ,
if(cf.FSeries is null,null,'*') as FSeries ,
if(cf.FCodeOKP is null,null,'*') as FCodeOKP
from usersettings.corefields cf join usersettings.pricesInfo pd on cf.pricecode=pd.pricecode
order by pd.SupplierName, pd.Region, pd.PriceName;
";
	}
}