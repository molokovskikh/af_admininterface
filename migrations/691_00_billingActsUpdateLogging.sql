
alter table Logs.ActLogs
add column CreatedOn datetime
;

DROP TRIGGER IF EXISTS billing.ActLogDelete;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER billing.ActLogDelete AFTER DELETE ON billing.Acts
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.ActLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		ActId = OLD.Id,
		Period = OLD.Period,
		ActDate = OLD.ActDate,
		Recipient = OLD.Recipient,
		Payer = OLD.Payer,
		Sum = OLD.Sum,
		PayerName = OLD.PayerName,
		Customer = OLD.Customer,
		CreatedOn = OLD.CreatedOn;
END;

DROP TRIGGER IF EXISTS billing.ActLogUpdate;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER billing.ActLogUpdate AFTER UPDATE ON billing.Acts
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.ActLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		ActId = OLD.Id,
		Period = NULLIF(NEW.Period, OLD.Period),
		ActDate = NULLIF(NEW.ActDate, OLD.ActDate),
		Recipient = NULLIF(NEW.Recipient, OLD.Recipient),
		Payer = NULLIF(NEW.Payer, OLD.Payer),
		Sum = NULLIF(NEW.Sum, OLD.Sum),
		PayerName = NULLIF(NEW.PayerName, OLD.PayerName),
		Customer = NULLIF(NEW.Customer, OLD.Customer),
		CreatedOn = NULLIF(NEW.CreatedOn, OLD.CreatedOn);
END;

DROP TRIGGER IF EXISTS billing.ActLogInsert;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER billing.ActLogInsert AFTER INSERT ON billing.Acts
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.ActLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		ActId = NEW.Id,
		Period = NEW.Period,
		ActDate = NEW.ActDate,
		Recipient = NEW.Recipient,
		Payer = NEW.Payer,
		Sum = NEW.Sum,
		PayerName = NEW.PayerName,
		Customer = NEW.Customer,
		CreatedOn = NEW.CreatedOn;
END;