
CREATE TABLE  `logs`.`SupplierPromotionLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `PromotionId` int(10) unsigned not null,
  `UpdateTime` timestamp,
  `Enabled` tinyint(1) unsigned,
  `SupplierId` int(10) unsigned,
  `Annotation` varchar(255),
  `PromoFile` varchar(255),
  `AgencyDisabled` tinyint(1) unsigned,
  `Name` varchar(255),
  `RegionMask` bigint(20) unsigned,
  `Begin` datetime,
  `End` datetime,
  `Status` tinyint(1) unsigned,

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

DROP TRIGGER IF EXISTS UserSettings.SupplierPromotionLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER UserSettings.SupplierPromotionLogDelete AFTER DELETE ON UserSettings.SupplierPromotions
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.SupplierPromotionLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		PromotionId = OLD.Id,
		UpdateTime = OLD.UpdateTime,
		Enabled = OLD.Enabled,
		SupplierId = OLD.SupplierId,
		Annotation = OLD.Annotation,
		PromoFile = OLD.PromoFile,
		AgencyDisabled = OLD.AgencyDisabled,
		Name = OLD.Name,
		RegionMask = OLD.RegionMask,
		Begin = OLD.Begin,
		End = OLD.End,
		Status = OLD.Status;
END;

DROP TRIGGER IF EXISTS UserSettings.SupplierPromotionLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER UserSettings.SupplierPromotionLogUpdate AFTER UPDATE ON UserSettings.SupplierPromotions
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.SupplierPromotionLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		PromotionId = OLD.Id,
		UpdateTime = NULLIF(NEW.UpdateTime, OLD.UpdateTime),
		Enabled = NULLIF(NEW.Enabled, OLD.Enabled),
		SupplierId = NULLIF(NEW.SupplierId, OLD.SupplierId),
		Annotation = NULLIF(NEW.Annotation, OLD.Annotation),
		PromoFile = NULLIF(NEW.PromoFile, OLD.PromoFile),
		AgencyDisabled = NULLIF(NEW.AgencyDisabled, OLD.AgencyDisabled),
		Name = NULLIF(NEW.Name, OLD.Name),
		RegionMask = NULLIF(NEW.RegionMask, OLD.RegionMask),
		Begin = NULLIF(NEW.Begin, OLD.Begin),
		End = NULLIF(NEW.End, OLD.End),
		Status = NULLIF(NEW.Status, OLD.Status);
END;

DROP TRIGGER IF EXISTS UserSettings.SupplierPromotionLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER UserSettings.SupplierPromotionLogInsert AFTER INSERT ON UserSettings.SupplierPromotions
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.SupplierPromotionLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		PromotionId = NEW.Id,
		UpdateTime = NEW.UpdateTime,
		Enabled = NEW.Enabled,
		SupplierId = NEW.SupplierId,
		Annotation = NEW.Annotation,
		PromoFile = NEW.PromoFile,
		AgencyDisabled = NEW.AgencyDisabled,
		Name = NEW.Name,
		RegionMask = NEW.RegionMask,
		Begin = NEW.Begin,
		End = NEW.End,
		Status = NEW.Status;
END;

