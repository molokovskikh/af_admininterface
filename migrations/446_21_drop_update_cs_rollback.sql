DROP PROCEDURE Usersettings.UpdateCS;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`UpdateCS`(
inOperatorName varchar(50),
inOperatorHost varchar(50),
inOperatorCode bigint(20),
inBrowser varchar(25),
inPriceCode int(32),
inClientCode int(32),
inIncludeType int(32),
inFirmClientCode1 varchar(200),
inFirmClientCode2 varchar(200),
inFirmClientCode3 varchar(200),
inPublicCostCorr decimal(6,3),
inFirmCostCorr decimal(6,3),
inInvisibleOnClient tinyint(1),
inCostCorrByClient tinyint(1),
inCostCode int(32),
inMinReq int(32),
inControlMinReq tinyint(1),
inID bigint(20),
inSlave tinyint(1)
)
BEGIN
set inFirmClientCode1 = trim(inFirmClientCode1);
set inFirmClientCode2 = trim(inFirmClientCode2);
set inFirmClientCode3 = trim(inFirmClientCode3);
UPDATE intersection
        SET PublicCostCorr= inPublicCostCorr,
        FirmCostCorr      = inFirmCostCorr,
        CostCode          = inCostCode,
        InvisibleOnClient = inInvisibleOnClient,
        CostCorrByClient  = inCostCorrByClient,
        MinReq            = inMinReq,
        ControlMinReq     = inControlMinReq
WHERE   id                = inID
        and not inSlave;
UPDATE intersection pc,
        includeregulation ir,
        intersection cc
        SET cc.PublicCostCorr= inPublicCostCorr,
        cc.FirmCostCorr      = inFirmCostCorr,
        cc.CostCode          = inCostCode,
        cc.InvisibleOnClient = if(ir.includetype = 1, cc.InvisibleOnClient, inInvisibleOnClient),
        cc.CostCorrByClient  = inCostCorrByClient,
        cc.MinReq            = if(ir.includetype = 1, cc.MinReq, inMinReq),
        cc.ControlMinReq     = if(ir.includetype = 1, cc.ControlMinReq, inControlMinReq)
WHERE   pc.id                = inID 
        and not inSlave
        and ir.primaryclientcode = pc.clientcode
        and cc.ClientCode        = ir.IncludeClientCode
        and cc.RegionCode        = pc.RegionCode
        and cc.PriceCode         = pc.PriceCode;
UPDATE intersection pc,
        includeregulation ir, 
        intersection cc 
        SET cc.FirmClientCode= inFirmClientCode1,
        cc.FirmClientCode2   = inFirmClientCode2, 
        cc.FirmClientCode3   = inFirmClientCode3 
WHERE   pc.id                = inID 
        and not inSlave 
        and ir.primaryclientcode = pc.clientcode 
        and cc.ClientCode        = ir.IncludeClientCode 
        and cc.RegionCode        = pc.RegionCode 
        and cc.PriceCode         = pc.PriceCode 
        and ir.IncludeType       = 2;
UPDATE intersection,
        pricesdata,
        pricesdata as a
        SET FirmClientCode        = if(length(inFirmClientCode1)> 0, inFirmClientCode1, FirmClientCode),
        FirmClientCode2           = if(length(inFirmClientCode2)> 0, inFirmClientCode2, FirmClientCode2),
        FirmClientCode3           = if(length(inFirmClientCode3)> 0, inFirmClientCode3, FirmClientCode3)
WHERE   intersection.clientcode   = inClientCode
        and intersection.pricecode= pricesdata.pricecode
        and pricesdata.firmcode   = a.firmcode
        and a.pricecode           = inPriceCode;
UPDATE intersection
        SET FirmClientCode= inFirmClientCode1,
        FirmClientCode2   = inFirmClientCode2,
        FirmClientCode3   = inFirmClientCode3,
        InvisibleOnClient = if(inSlave and inIncludeType = 1, inInvisibleOnClient, InvisibleOnClient),
        MinReq            = if(inSlave and inIncludeType = 1, inMinReq, MinReq),
        ControlMinReq     = if(inSlave and inIncludeType = 1, inControlMinReq, ControlMinReq)
WHERE   id                = inID;
END;
