
alter table Logs.SupplierLogs
add column Account int(10) unsigned
;

DROP TRIGGER IF EXISTS Future.SupplierLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.SupplierLogDelete AFTER DELETE ON Future.Suppliers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.SupplierLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		SupplierId = OLD.Id,
		Name = OLD.Name,
		FullName = OLD.FullName,
		RegionMask = OLD.RegionMask,
		Disabled = OLD.Disabled,
		Registrant = OLD.Registrant,
		RegistrationDate = OLD.RegistrationDate,
		HomeRegion = OLD.HomeRegion,
		ContactGroupOwnerId = OLD.ContactGroupOwnerId,
		Payer = OLD.Payer,
		Account = OLD.Account;
END;

DROP TRIGGER IF EXISTS Future.SupplierLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.SupplierLogUpdate AFTER UPDATE ON Future.Suppliers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.SupplierLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		SupplierId = OLD.Id,
		Name = NULLIF(NEW.Name, OLD.Name),
		FullName = NULLIF(NEW.FullName, OLD.FullName),
		RegionMask = NULLIF(NEW.RegionMask, OLD.RegionMask),
		Disabled = NULLIF(NEW.Disabled, OLD.Disabled),
		Registrant = NULLIF(NEW.Registrant, OLD.Registrant),
		RegistrationDate = NULLIF(NEW.RegistrationDate, OLD.RegistrationDate),
		HomeRegion = NULLIF(NEW.HomeRegion, OLD.HomeRegion),
		ContactGroupOwnerId = NULLIF(NEW.ContactGroupOwnerId, OLD.ContactGroupOwnerId),
		Payer = NULLIF(NEW.Payer, OLD.Payer),
		Account = NULLIF(NEW.Account, OLD.Account);
END;

DROP TRIGGER IF EXISTS Future.SupplierLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.SupplierLogInsert AFTER INSERT ON Future.Suppliers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.SupplierLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		SupplierId = NEW.Id,
		Name = NEW.Name,
		FullName = NEW.FullName,
		RegionMask = NEW.RegionMask,
		Disabled = NEW.Disabled,
		Registrant = NEW.Registrant,
		RegistrationDate = NEW.RegistrationDate,
		HomeRegion = NEW.HomeRegion,
		ContactGroupOwnerId = NEW.ContactGroupOwnerId,
		Payer = NEW.Payer,
		Account = NEW.Account;
END;

