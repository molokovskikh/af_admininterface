DROP TRIGGER IF EXISTS Billing.LegalEntityLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.LegalEntityLogDelete AFTER DELETE ON Billing.LegalEntities
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.LegalEntityLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		EntityId = OLD.Id,
		PayerId = OLD.PayerId,
		Name = OLD.Name,
		FullName = OLD.FullName;
END;

DROP TRIGGER IF EXISTS Billing.LegalEntityLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.LegalEntityLogUpdate AFTER UPDATE ON Billing.LegalEntities
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.LegalEntityLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		EntityId = OLD.Id,
		PayerId = NULLIF(NEW.PayerId, OLD.PayerId),
		Name = NULLIF(NEW.Name, OLD.Name),
		FullName = NULLIF(NEW.FullName, OLD.FullName);
END;

DROP TRIGGER IF EXISTS Billing.LegalEntityLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.LegalEntityLogInsert AFTER INSERT ON Billing.LegalEntities
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.LegalEntityLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		EntityId = NEW.Id,
		PayerId = NEW.PayerId,
		Name = NEW.Name,
		FullName = NEW.FullName;
END;

