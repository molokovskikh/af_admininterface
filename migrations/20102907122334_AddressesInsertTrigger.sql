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
		Free = NEW.Free,
		Address = NEW.Address,
		ContactGroupId = NEW.ContactGroupId,
		AccountingId = NEW.AccountingId;
END;
;
