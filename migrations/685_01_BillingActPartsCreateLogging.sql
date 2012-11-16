
CREATE TABLE  `logs`.`ActPartLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `PartId` int(10) unsigned not null,
  `Name` varchar(255),
  `Cost` decimal(19,5),
  `Count` int(11),
  `Act` int(10) unsigned,

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

DROP TRIGGER IF EXISTS Billing.ActPartLogDelete;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.ActPartLogDelete AFTER DELETE ON Billing.ActParts
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.ActPartLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		PartId = OLD.Id,
		Name = OLD.Name,
		Cost = OLD.Cost,
		Count = OLD.Count,
		Act = OLD.Act;
END;

DROP TRIGGER IF EXISTS Billing.ActPartLogUpdate;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.ActPartLogUpdate AFTER UPDATE ON Billing.ActParts
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.ActPartLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		PartId = OLD.Id,
		Name = NULLIF(NEW.Name, OLD.Name),
		Cost = NULLIF(NEW.Cost, OLD.Cost),
		Count = NULLIF(NEW.Count, OLD.Count),
		Act = NULLIF(NEW.Act, OLD.Act);
END;

DROP TRIGGER IF EXISTS Billing.ActPartLogInsert;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.ActPartLogInsert AFTER INSERT ON Billing.ActParts
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.ActPartLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		PartId = NEW.Id,
		Name = NEW.Name,
		Cost = NEW.Cost,
		Count = NEW.Count,
		Act = NEW.Act;
END;
