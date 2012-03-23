drop trigger Future.SupplierLogUpdate;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Future.SupplierLogUpdate AFTER UPDATE ON Future.Suppliers
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
		Payer = NULLIF(NEW.Payer, OLD.Payer);

	if NEW.Disabled is not null then
		update Usersettings.ClientsData set FirmStatus = if(NEW.Disabled = 1, 0, 1) where FirmCode = NEW.Id;
	end if;
END;
