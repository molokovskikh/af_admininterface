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
		Free = NULLIF(NEW.Free, OLD.Free),
		Address = NULLIF(NEW.Address, OLD.Address),
		ContactGroupId = NULLIF(NEW.ContactGroupId, OLD.ContactGroupId),
		AccountingId = NULLIF(NEW.AccountingId, OLD.AccountingId);
END;
;
