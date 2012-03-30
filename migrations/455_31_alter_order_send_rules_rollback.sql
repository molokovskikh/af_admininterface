DROP PROCEDURE OrderSendRules.GetOrderHeader;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE OrderSendRules.`GetOrderHeader`(IN idparam  integer unsigned)
BEGIN

SELECT   Writetime + interval reg.MoscowBias hour Writetime     ,
         oh.ClientCode                                          ,
         PriceDate + interval reg.MoscowBias hour PriceDate     ,
         clientAddition                           ClientComment ,
         RowCount                                               ,
         oh.PriceCode                                           ,
         if(count(ifu.LegalEntityId)>1, le.Name, cd.name) ClientShortName                           ,
         if(count(ifu.LegalEntityId)>1, le.FullName, cd.Fullname) ClientFullName  ,
         a.Address ClientAddress,
         (SELECT c.contactText
         FROM    contacts.contact_groups cg
          JOIN contacts.contacts c
                 ON      cg.Id          = c.ContactOwnerId
         WHERE   cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
             AND cg.Type = 0
             AND c.Type = 1 
         limit 1
         ) AS ClientPhone,
         (SELECT p.Name
         FROM contacts.contact_groups cg
             join contacts.Persons p on p.ContactGroupId = cg.Id 
         WHERE cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
             AND cg.Type = 2
         limit 1
         ) AS ClientContactName,
         if(ifnull(os.SwapFirmCode, 0), ai.SupplierDeliveryId, i.SupplierClientId) FirmClientCode,
         if(ifnull(os.SwapFirmCode, 0), i.SupplierClientId, ai.SupplierDeliveryId)  FirmClientCode2,
         i.SupplierPaymentId FirmClientCode3,
         0 PublicCostCorr ,
         MIN(i.PriceMarkup)   FirmCostCorr   ,
         (SELECT ROUND(SUM(ol.cost*ol.Quantity),2)
         FROM    orders.orderslist AS ol
         WHERE   ol.orderid= oh.RowId
         )  AS Summ,
         s.Name as FirmShortName,
         cd.regionCode ClientRegionCode,
         pd.FirmCode,
         pd.pricename PriceName,
         region RegionName,
         rcs.SendRetailMarkup as SendRetailMarkup,
         rcs.ServiceClient as SendOrdersToTech,
         rcs.InvisibleOnFirm = 2 or rcs.FirmCodeOnly is not null or pd.PriceCode = 2355 as SendOrdersToOffice,
         u.PayerId = 921 or a.PayerId = 921 as FakeOrder,
         a.PayerId as BillingCode,
         pc.CostName,
         oh.RowId as OrderId,
         oh.AddressId,
         oh.UserId,
  exists (
   select *
   from Usersettings.PricesData pd
    join Usersettings.CostOptimizationRules cor on cor.SupplierId = pd.FirmCode
     join Usersettings.CostOptimizationClients coc on coc.RuleId = cor.Id
   where coc.ClientId = oh.ClientCode and pd.PriceCode = oh.PriceCode
  ) as IsCostOptimizationEnabled
FROM     (Future.Clients AS cd ,
         Future.Suppliers AS s,
         usersettings.pricesdata AS pd ,
         usersettings.regionaldata AS rd ,
         farm.regions AS reg,
         usersettings.retclientsset AS rcs,
         orders.ordershead AS oh)
  join Future.Addresses a on a.Id = oh.AddressId 
  join Future.Users u on u.Id = oh.UserId
  left join Future.Intersection i ON i.PriceId  = oh.PriceCode AND i.RegionId = oh.regionCode AND i.ClientId = oh.ClientCode and i.LegalEntityId = a.LegalEntityId
  left join Future.AddressIntersection ai on ai.AddressId = a.Id and ai.IntersectionId = i.Id
  join Billing.LegalEntities le on le.Id = a.LegalEntityId
  join Future.Intersection ifu on ifu.ClientId=oh.ClientCode and ifu.PriceId  = oh.PriceCode AND ifu.RegionId = oh.regionCode
  left join usersettings.pricescosts pc ON pc.costcode = i.CostId
  left join Future.OrderSwap os on os.ClientId = oh.ClientCode and os.SupplierId = s.Id
WHERE cd.Id = oh.ClientCode
     AND oh.PriceCode = pd.PriceCode
     AND s.Id = pd.FirmCode
     AND rd.regionCode = oh.regionCode
     AND rd.firmCode = pd.FirmCode
     AND reg.regioncode = cd.regioncode
     AND cd.Id = rcs.clientcode
     AND oh.rowid = idparam
GROUP BY oh.RowId;

END;
