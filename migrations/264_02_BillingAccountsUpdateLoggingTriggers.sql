DROP TRIGGER IF EXISTS Billing.AccountLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.AccountLogDelete AFTER DELETE ON Billing.Accounts
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AccountLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		AccountId = OLD.Id,
		WriteTime = OLD.WriteTime,
		Type = OLD.Type,
		Operator = OLD.Operator,
		Payment = OLD.Payment,
		BeAccounted = OLD.BeAccounted,
		ReadyForAcounting = OLD.ReadyForAcounting,
		InvoiceGroup = OLD.InvoiceGroup,
		IsFree = OLD.IsFree,
		ObjectId = OLD.ObjectId,
		Description = OLD.Description;
END;

DROP TRIGGER IF EXISTS Billing.AccountLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.AccountLogUpdate AFTER UPDATE ON Billing.Accounts
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AccountLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		AccountId = OLD.Id,
		WriteTime = NULLIF(NEW.WriteTime, OLD.WriteTime),
		Type = NULLIF(NEW.Type, OLD.Type),
		Operator = NULLIF(NEW.Operator, OLD.Operator),
		Payment = NULLIF(NEW.Payment, OLD.Payment),
		BeAccounted = NULLIF(NEW.BeAccounted, OLD.BeAccounted),
		ReadyForAcounting = NULLIF(NEW.ReadyForAcounting, OLD.ReadyForAcounting),
		InvoiceGroup = NULLIF(NEW.InvoiceGroup, OLD.InvoiceGroup),
		IsFree = NULLIF(NEW.IsFree, OLD.IsFree),
		ObjectId = NULLIF(NEW.ObjectId, OLD.ObjectId),
		Description = NULLIF(NEW.Description, OLD.Description);
END;

DROP TRIGGER IF EXISTS Billing.AccountLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.AccountLogInsert AFTER INSERT ON Billing.Accounts
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.AccountLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		AccountId = NEW.Id,
		WriteTime = NEW.WriteTime,
		Type = NEW.Type,
		Operator = NEW.Operator,
		Payment = NEW.Payment,
		BeAccounted = NEW.BeAccounted,
		ReadyForAcounting = NEW.ReadyForAcounting,
		InvoiceGroup = NEW.InvoiceGroup,
		IsFree = NEW.IsFree,
		ObjectId = NEW.ObjectId,
		Description = NEW.Description;
END;

