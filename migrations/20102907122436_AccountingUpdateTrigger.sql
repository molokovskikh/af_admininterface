DROP TRIGGER IF EXISTS Billing.AccountingLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.AccountingLogUpdate AFTER UPDATE ON Billing.Accounting
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AccountingLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		AccountingId = OLD.Id,
		WriteTime = NULLIF(NEW.WriteTime, OLD.WriteTime),
		Type = NULLIF(NEW.Type, OLD.Type),
		Operator = NULLIF(NEW.Operator, OLD.Operator),
		Payment = NULLIF(NEW.Payment, OLD.Payment),
		BeAccounted = NULLIF(NEW.BeAccounted, OLD.BeAccounted),
		ReadyForAcounting = NULLIF(NEW.ReadyForAcounting, OLD.ReadyForAcounting);
END;
;
