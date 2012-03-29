
CREATE TABLE  `logs`.`PricesDatumLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `PriceCode` int(10) unsigned,
  `FirmCode` int(11) unsigned,
  `RegionMask` bigint(20) unsigned,
  `AgencyEnabled` tinyint(1) unsigned,
  `Enabled` tinyint(1) unsigned,
  `PriceType` tinyint(1) unsigned,
  `PriceName` varchar(50),
  `MinReq` int(11) unsigned,
  `UpCost` decimal(5,2),
  `PriceInfo` text,
  `CostType` tinyint(1) unsigned,
  `ParentSynonym` int(11) unsigned,
  `BuyingMatrix` tinyint(1),

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

DROP TRIGGER IF EXISTS Usersettings.PricesDatumLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.PricesDatumLogDelete AFTER DELETE ON Usersettings.PricesData
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PricesDatumLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		PriceCode = OLD.PriceCode,
		FirmCode = OLD.FirmCode,
		RegionMask = OLD.RegionMask,
		AgencyEnabled = OLD.AgencyEnabled,
		Enabled = OLD.Enabled,
		PriceType = OLD.PriceType,
		PriceName = OLD.PriceName,
		MinReq = OLD.MinReq,
		UpCost = OLD.UpCost,
		PriceInfo = OLD.PriceInfo,
		CostType = OLD.CostType,
		ParentSynonym = OLD.ParentSynonym,
		BuyingMatrix = OLD.BuyingMatrix;
END;

DROP TRIGGER IF EXISTS Usersettings.PricesDatumLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.PricesDatumLogUpdate AFTER UPDATE ON Usersettings.PricesData
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PricesDatumLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		PriceCode = OLD.PriceCode,
		FirmCode = NULLIF(NEW.FirmCode, OLD.FirmCode),
		RegionMask = NULLIF(NEW.RegionMask, OLD.RegionMask),
		AgencyEnabled = NULLIF(NEW.AgencyEnabled, OLD.AgencyEnabled),
		Enabled = NULLIF(NEW.Enabled, OLD.Enabled),
		PriceType = NULLIF(NEW.PriceType, OLD.PriceType),
		PriceName = NULLIF(NEW.PriceName, OLD.PriceName),
		MinReq = NULLIF(NEW.MinReq, OLD.MinReq),
		UpCost = NULLIF(NEW.UpCost, OLD.UpCost),
		PriceInfo = NULLIF(NEW.PriceInfo, OLD.PriceInfo),
		CostType = NULLIF(NEW.CostType, OLD.CostType),
		ParentSynonym = NULLIF(NEW.ParentSynonym, OLD.ParentSynonym),
		BuyingMatrix = NULLIF(NEW.BuyingMatrix, OLD.BuyingMatrix);
END;

DROP TRIGGER IF EXISTS Usersettings.PricesDatumLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.PricesDatumLogInsert AFTER INSERT ON Usersettings.PricesData
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PricesDatumLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		PriceCode = NEW.PriceCode,
		FirmCode = NEW.FirmCode,
		RegionMask = NEW.RegionMask,
		AgencyEnabled = NEW.AgencyEnabled,
		Enabled = NEW.Enabled,
		PriceType = NEW.PriceType,
		PriceName = NEW.PriceName,
		MinReq = NEW.MinReq,
		UpCost = NEW.UpCost,
		PriceInfo = NEW.PriceInfo,
		CostType = NEW.CostType,
		ParentSynonym = NEW.ParentSynonym,
		BuyingMatrix = NEW.BuyingMatrix;
END;

