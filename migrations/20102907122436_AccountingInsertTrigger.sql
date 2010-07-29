DROP TRIGGER IF EXISTS Billing.AccountingLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.AccountingLogInsert AFTER INSERT ON Billing.Accounting
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AccountingLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		AccountingId = NEW.Id,
		WriteTime = NEW.WriteTime,
		Type = NEW.Type,
		Operator = NEW.Operator,
		Payment = NEW.Payment,
		BeAccounted = NEW.BeAccounted,
		ReadyForAcounting = NEW.ReadyForAcounting;
END;
;
