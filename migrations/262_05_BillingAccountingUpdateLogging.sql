
alter table Logs.AccountingLogs
add column InvoiceGroup int(11),
add column IsFree tinyint(1),
add column ObjectId int(10) unsigned
;

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
		ReadyForAcounting = OLD.ReadyForAcounting,
		InvoiceGroup = OLD.InvoiceGroup,
		IsFree = OLD.IsFree,
		ObjectId = OLD.ObjectId;
END;

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
		ReadyForAcounting = NULLIF(NEW.ReadyForAcounting, OLD.ReadyForAcounting),
		InvoiceGroup = NULLIF(NEW.InvoiceGroup, OLD.InvoiceGroup),
		IsFree = NULLIF(NEW.IsFree, OLD.IsFree),
		ObjectId = NULLIF(NEW.ObjectId, OLD.ObjectId);
END;

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
		ReadyForAcounting = NEW.ReadyForAcounting,
		InvoiceGroup = NEW.InvoiceGroup,
		IsFree = NEW.IsFree,
		ObjectId = NEW.ObjectId;
END;

