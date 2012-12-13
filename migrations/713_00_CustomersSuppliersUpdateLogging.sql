
alter table Logs.SupplierLogs
add column Address varchar(255),
add column VendorId varchar(255)
;

DROP TRIGGER IF EXISTS Customers.SupplierLogDelete;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.SupplierLogDelete AFTER DELETE ON Customers.Suppliers
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
		Account = OLD.Account,
		Address = OLD.Address,
		VendorId = OLD.VendorId;
END;

DROP TRIGGER IF EXISTS Customers.SupplierLogUpdate;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.SupplierLogUpdate AFTER UPDATE ON Customers.Suppliers
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
		Account = NULLIF(NEW.Account, OLD.Account),
		Address = NULLIF(NEW.Address, OLD.Address),
		VendorId = NULLIF(NEW.VendorId, OLD.VendorId);
END;

DROP TRIGGER IF EXISTS Customers.SupplierLogInsert;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.SupplierLogInsert AFTER INSERT ON Customers.Suppliers
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
		Account = NEW.Account,
		Address = NEW.Address,
		VendorId = NEW.VendorId;
END;