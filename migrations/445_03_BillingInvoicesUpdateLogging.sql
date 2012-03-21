
alter table Logs.InvoiceLogs
add column BalanceAmount decimal(19,5)
;

DROP TRIGGER IF EXISTS Billing.InvoiceLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.InvoiceLogDelete AFTER DELETE ON Billing.Invoices
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.InvoiceLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		InvoiceId = OLD.Id,
		Sum = OLD.Sum,
		Date = OLD.Date,
		Period = OLD.Period,
		Recipient = OLD.Recipient,
		Payer = OLD.Payer,
		CreatedOn = OLD.CreatedOn,
		SendToEmail = OLD.SendToEmail,
		LastErrorNotification = OLD.LastErrorNotification,
		PayerName = OLD.PayerName,
		Act = OLD.Act,
		Customer = OLD.Customer,
		PaidSum = OLD.PaidSum,
		SendToMinimail = OLD.SendToMinimail,
		BalanceAmount = OLD.BalanceAmount;
END;

DROP TRIGGER IF EXISTS Billing.InvoiceLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.InvoiceLogUpdate AFTER UPDATE ON Billing.Invoices
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.InvoiceLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		InvoiceId = OLD.Id,
		Sum = NULLIF(NEW.Sum, OLD.Sum),
		Date = NULLIF(NEW.Date, OLD.Date),
		Period = NULLIF(NEW.Period, OLD.Period),
		Recipient = NULLIF(NEW.Recipient, OLD.Recipient),
		Payer = NULLIF(NEW.Payer, OLD.Payer),
		CreatedOn = NULLIF(NEW.CreatedOn, OLD.CreatedOn),
		SendToEmail = NULLIF(NEW.SendToEmail, OLD.SendToEmail),
		LastErrorNotification = NULLIF(NEW.LastErrorNotification, OLD.LastErrorNotification),
		PayerName = NULLIF(NEW.PayerName, OLD.PayerName),
		Act = NULLIF(NEW.Act, OLD.Act),
		Customer = NULLIF(NEW.Customer, OLD.Customer),
		PaidSum = NULLIF(NEW.PaidSum, OLD.PaidSum),
		SendToMinimail = NULLIF(NEW.SendToMinimail, OLD.SendToMinimail),
		BalanceAmount = NULLIF(NEW.BalanceAmount, OLD.BalanceAmount);
END;

DROP TRIGGER IF EXISTS Billing.InvoiceLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.InvoiceLogInsert AFTER INSERT ON Billing.Invoices
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.InvoiceLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		InvoiceId = NEW.Id,
		Sum = NEW.Sum,
		Date = NEW.Date,
		Period = NEW.Period,
		Recipient = NEW.Recipient,
		Payer = NEW.Payer,
		CreatedOn = NEW.CreatedOn,
		SendToEmail = NEW.SendToEmail,
		LastErrorNotification = NEW.LastErrorNotification,
		PayerName = NEW.PayerName,
		Act = NEW.Act,
		Customer = NEW.Customer,
		PaidSum = NEW.PaidSum,
		SendToMinimail = NEW.SendToMinimail,
		BalanceAmount = NEW.BalanceAmount;
END;

