DROP TRIGGER IF EXISTS Billing.AccountingLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.AccountingLogDelete AFTER DELETE ON Billing.Accounting
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AccountingLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		AccountingId = OLD.Id,
		WriteTime = OLD.WriteTime,
		Type = OLD.Type,
		Operator = OLD.Operator,
		Payment = OLD.Payment,
		BeAccounted = OLD.BeAccounted,
		ReadyForAcounting = OLD.ReadyForAcounting;
END;
;
