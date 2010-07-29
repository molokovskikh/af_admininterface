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
		Free = OLD.Free,
		Address = OLD.Address,
		ContactGroupId = OLD.ContactGroupId,
		AccountingId = OLD.AccountingId;
END;
;
