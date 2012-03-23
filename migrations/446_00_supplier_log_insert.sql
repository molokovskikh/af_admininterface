drop trigger Future.SupplierLogInsert;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Future.SupplierLogInsert AFTER INSERT ON Future.Suppliers
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
		Payer = NEW.Payer;
END;
