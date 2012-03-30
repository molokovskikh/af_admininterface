DROP PROCEDURE Future.AFGetActivePrices;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Future.`AFGetActivePrices`(IN UserIdParam INT UNSIGNED)
BEGIN

Declare TabelExsists Bool DEFAULT false;
Declare TablePricesExists Bool default false;
DECLARE CONTINUE HANDLER FOR 1146
begin
  if not TablePricesExists then
    Call Future.GetPrices(UserIdParam);
  else
    resignal;
  end if;
end;

if not TabelExsists then
DROP TEMPORARY TABLE IF EXISTS Usersettings.ActivePrices;
create temporary table
Usersettings.ActivePrices
(
 FirmCode int Unsigned,
 PriceCode int Unsigned,
 CostCode int Unsigned,
 PriceSynonymCode int Unsigned,
 RegionCode BigInt Unsigned,
 Fresh bool,
 Upcost decimal(7,5),
 MaxSynonymCode Int Unsigned,
 MaxSynonymFirmCrCode Int Unsigned,
 CostType bool,
 PriceDate DateTime,
 ShowPriceName bool,
 PriceName VarChar(50),
 PositionCount int Unsigned,
 MinReq mediumint Unsigned,
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

select null from Usersettings.Prices limit 0;
if not TablePricesExists then
  set TablePricesExists = true;
end if;

INSERT
INTO Usersettings.ActivePrices(
 FirmCode,
 PriceCode,
 CostCode,
 PriceSynonymCode,
 RegionCode,
 Fresh,
 Upcost,
 MaxSynonymCode,
 MaxSynonymFirmCrCode,
 CostType,
 PriceDate,
 ShowPriceName,
 PriceName,
 PositionCount,
 MinReq,
 FirmCategory,
 MainFirm)
SELECT P.FirmCode,
       P.PriceCode,
       P.CostCode,
       P.PriceSynonymCode,
       P.RegionCode,
       RI.ForceReplication !=0,
       P.Upcost,
       RI.MaxSynonymCode,
       RI.MaxSynonymFirmCrCode,
       P.CostType,
       P.PriceDate,
       P.ShowPriceName,
       P.PriceName,
       P.PositionCount,
       P.MinReq,
       P.FirmCategory,
       P.MainFirm
FROM 
  Usersettings.Prices P
  join Usersettings.CurrentReplicationInfo RI on RI.FirmCode = P.FirmCode
WHERE  p.Actual = 1
  and p.DisabledByClient = 0
  and RI.UserId = UserIdParam;

END;
