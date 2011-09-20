DROP TRIGGER IF EXISTS Future.AddressLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.AddressLogDelete AFTER DELETE ON Future.Addresses
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AddressLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		AddressId = OLD.Id,
		LegacyId = OLD.LegacyId,
		ClientId = OLD.ClientId,
		LegalEntityId = OLD.LegalEntityId,
		Enabled = OLD.Enabled,
		Address = OLD.Address,
		ContactGroupId = OLD.ContactGroupId,
		AccountingId = OLD.AccountingId,
		PayerId = OLD.PayerId,
		Registrant = OLD.Registrant,
		RegistrationDate = OLD.RegistrationDate;
END;

DROP TRIGGER IF EXISTS Future.AddressLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.AddressLogUpdate AFTER UPDATE ON Future.Addresses
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AddressLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		AddressId = OLD.Id,
		LegacyId = NULLIF(NEW.LegacyId, OLD.LegacyId),
		ClientId = NULLIF(NEW.ClientId, OLD.ClientId),
		LegalEntityId = NULLIF(NEW.LegalEntityId, OLD.LegalEntityId),
		Enabled = NULLIF(NEW.Enabled, OLD.Enabled),
		Address = NULLIF(NEW.Address, OLD.Address),
		ContactGroupId = NULLIF(NEW.ContactGroupId, OLD.ContactGroupId),
		AccountingId = NULLIF(NEW.AccountingId, OLD.AccountingId),
		PayerId = NULLIF(NEW.PayerId, OLD.PayerId),
		Registrant = NULLIF(NEW.Registrant, OLD.Registrant),
		RegistrationDate = NULLIF(NEW.RegistrationDate, OLD.RegistrationDate);
END;

DROP TRIGGER IF EXISTS Future.AddressLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.AddressLogInsert AFTER INSERT ON Future.Addresses
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AddressLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		AddressId = NEW.Id,
		LegacyId = NEW.LegacyId,
		ClientId = NEW.ClientId,
		LegalEntityId = NEW.LegalEntityId,
		Enabled = NEW.Enabled,
		Address = NEW.Address,
		ContactGroupId = NEW.ContactGroupId,
		AccountingId = NEW.AccountingId,
		PayerId = NEW.PayerId,
		Registrant = NEW.Registrant,
		RegistrationDate = NEW.RegistrationDate;
END;

