
CREATE TABLE  `logs`.`PromotionCatalogLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `CatalogId` int(10) unsigned,
  `PromotionId` int(10) unsigned,
  `UpdateTime` timestamp,

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

DROP TRIGGER IF EXISTS UserSettings.PromotionCatalogLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER UserSettings.PromotionCatalogLogDelete AFTER DELETE ON UserSettings.PromotionCatalogs
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PromotionCatalogLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		CatalogId = OLD.CatalogId,
		PromotionId = OLD.PromotionId,
		UpdateTime = OLD.UpdateTime;
END;

DROP TRIGGER IF EXISTS UserSettings.PromotionCatalogLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER UserSettings.PromotionCatalogLogUpdate AFTER UPDATE ON UserSettings.PromotionCatalogs
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PromotionCatalogLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		CatalogId = NULLIF(NEW.CatalogId, OLD.CatalogId),
		PromotionId = NULLIF(NEW.PromotionId, OLD.PromotionId),
		UpdateTime = NULLIF(NEW.UpdateTime, OLD.UpdateTime);
END;

DROP TRIGGER IF EXISTS UserSettings.PromotionCatalogLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER UserSettings.PromotionCatalogLogInsert AFTER INSERT ON UserSettings.PromotionCatalogs
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PromotionCatalogLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		CatalogId = NEW.CatalogId,
		PromotionId = NEW.PromotionId,
		UpdateTime = NEW.UpdateTime;
END;

