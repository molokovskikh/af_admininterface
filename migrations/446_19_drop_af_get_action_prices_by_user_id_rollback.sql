DROP PROCEDURE if exists Usersettings.AFGetActivePricesByUserId;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`AFGetActivePricesByUserId`(IN UserIdParam INT UNSIGNED)
BEGIN

Declare TabelExsists Bool DEFAULT false;
Declare ClientCodeParam INT UNSIGNED;

Declare TablePricesExists Bool default false;
DECLARE CONTINUE HANDLER FOR 1146
begin
  if not TablePricesExists then
    SELECT ClientCode
    INTO   ClientCodeParam
    FROM   OsUserAccessRight
    WHERE  RowId=UserIdParam;
    Call GetPrices2(ClientCodeParam);
  else
    resignal;
  end if;
end;

if not TabelExsists then
DROP TEMPORARY TABLE IF EXISTS  ActivePrices;
create temporary table
ActivePrices
(

 FirmCode int Unsigned,
 PriceCode int Unsigned,
 CostCode int Unsigned,
 PriceSynonymCode int Unsigned,
 RegionCode BigInt Unsigned,
 Fresh bool,
 Upcost decimal(7,5),
 PublicUpCost decimal(7,5),
 MaxSynonymCode Int Unsigned,
 MaxSynonymFirmCrCode Int Unsigned,
 CostType bool,
 PriceDate DateTime,
 ShowPriceName bool,
 PriceName VarChar(50),
 PositionCount int Unsigned,
 MinReq mediumint Unsigned,
 CostCorrByClient bool,
 FirmCategory tinyint unsigned,
 MainFirm bool,
 unique (PriceCode, RegionCode, CostCode),
 index  (CostCode, PriceCode),
 index  (PriceSynonymCode),
 index  (MaxSynonymCode),
 index  (PriceCode),
 index  (MaxSynonymFirmCrCode)
 )engine=MEMORY
 ;
set TabelExsists=true;
end if;

select null from Prices limit 0;
if not TablePricesExists then
  set TablePricesExists = true;
end if;

INSERT
INTO    ActivePrices
        (
 FirmCode,
 PriceCode,
 CostCode,
 PriceSynonymCode,
 RegionCode,
 Fresh,
 Upcost,
 PublicUpCost,
 MaxSynonymCode,
 MaxSynonymFirmCrCode,
 CostType,
 PriceDate,
 ShowPriceName,
 PriceName,
 PositionCount,
 MinReq,
 CostCorrByClient,
 FirmCategory,
 MainFirm
        ) 
SELECT P.FirmCode            ,
       P.PriceCode           ,
       P.CostCode            ,
       P.PriceSynonymCode    ,
       P.RegionCode          ,
       RI.ForceReplication !=0,
       P.Upcost              ,
       P.PublicUpCost        ,
       RI.MaxSynonymCode      ,
       RI.MaxSynonymFirmCrCode,
       P.CostType            ,
       P.PriceDate           ,
       P.ShowPriceName       ,
       P.PriceName           ,
       P.PositionCount       ,
       P.MinReq              ,
       P.CostCorrByClient    ,
       P.FirmCategory        ,
       P.MainFirm
FROM   Prices P,
       Usersettings.CurrentReplicationInfo RI
WHERE  p.DisabledByClient=0
   AND p.Actual          =1
   AND RI.UserId        =UserIdParam
   AND RI.FirmCode      =P.FirmCode;

END;
