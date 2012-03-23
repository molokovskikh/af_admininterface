drop procedure if exists Usersettings.SelectCS;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`SelectCS`(IN inRegCode BIGINT(20), IN inUserCode INTEGER, IN inPriceCode INTEGER)
BEGIN
SELECT  intersection.id,
        concat(a.shortname, IF(primaryClientCode is not null, concat(' (', b.shortname, ')'), '')) as shortname,
        intersection.clientcode,
        if(c.disabledbyagency= 1 or c.InvisibleOnFirm = 1, 1, 0)  as Inactive,
        if(c.disabledbyfirm is null, 0, c.disabledbyfirm)                                                                                                                                                  as disabledbyfirm,
        intersection.firmclientcode                                                                                                                                                                        as FirmClientCode1,
        if(c.publiccostcorr is null, 0, c.publiccostcorr)        as publiccostcorr,
        if(c.costcorrbyclient is null, 1, c.costcorrbyclient)    as costcorrbyclient,
        if((primaryclientcode is not null) and ((includetype = 1)), if(intersection.InvisibleOnClient is null, 0, intersection.InvisibleOnClient), if(c.InvisibleOnClient is null, 0, c.InvisibleOnClient)) as InvisibleOnClient,
        usersettings.GetClientUpdateTime(a.FirmCode)                                                                                                                                                        as UpdateTime,
        intersection.pricecode,
        PriceType,
        if((primaryClientCode is not null) and (includetype in (0, 3)), ifnull(c.disabledbyclient, 0)=1, ifnull(intersection.disabledbyclient, 0)= 1) as DisabledByClient,
        primaryClientCode is not null                                                                                   as SlaveClient,
        if(c.firmcostcorr is null, 0, c.firmcostcorr)                                                                   as firmcostcorr,
        intersection.FirmClientCode2,
        intersection.FirmClientCode3,
        ifnull(CostName, '-') CostName,
        pc.CostCode,
        if((primaryClientCode is not null) and (includetype in (0, 3)), c.minreq, intersection.minreq) as minreq,
        if((primaryClientCode is not null) and (includetype in (0, 3)), ifnull(c.controlminreq, 0), ifnull(intersection.controlminreq, 0)) as controlminreq,
        intersection.invisibleonfirm,
        ir.primaryclientcode,

        if(ir.includetype in (0, 3), 0, includetype) as includetype
FROM    (intersection, pricesdata, clientsdata, retclientsset)
LEFT JOIN clientsdata as a
        ON a.firmcode      = intersection.clientcode
        and a.billingstatus= 1
        and a.firmsegment  = clientsdata.firmsegment
        and a.firmstatus   = 1
LEFT JOIN includeregulation ir
        ON IncludeClientCode = intersection.clientcode
        and not exists(select * from includeregulation c where c.IncludeClientCode = ir.PrimaryClientCode and c.IncludeType = 2)
LEFT JOIN clientsdata as b
        ON b.firmcode      = primaryClientCode
        and b.firmstatus   = 1
        and b.billingstatus= 1
LEFT JOIN retclientsset as rcs
        ON rcs.clientcode= primaryClientCode
LEFT JOIN intersection as c
        ON c.pricecode  = intersection.pricecode
        and c.regioncode= intersection.regioncode
        and c.clientcode= if(primaryclientcode is null, intersection.clientcode, primaryclientcode)
LEFT JOIN PricesCosts pc
        ON pc.costcode                                        = c.costcode
WHERE   pricesdata.pricecode                                  = intersection.pricecode
        and clientsdata.firmcode                              = pricesdata.firmcode
        and retclientsset.clientcode                          = a.firmcode
        and intersection.regioncode                           = inRegCode
        and (clientsdata.maskregion & intersection.regioncode)> 0
        and clientsdata.firmcode                              = inUserCode
        and pricesdata.pricecode                              = inPriceCode
        and clientsdata.firmstatus                            = 1
        and clientsdata.billingstatus                         = 1
        and intersection.invisibleonfirm                      < 2
        and retclientsset.invisibleonfirm                     < 2
        and clientsdata.firmtype                              = 0
        and (a.maskregion & intersection.regioncode)          > 0
        and ((primaryclientcode is not null and (includetype in (0, 1, 3))) or (primaryclientcode is null))
        and intersection.DisabledByAgency = 0;
END;
