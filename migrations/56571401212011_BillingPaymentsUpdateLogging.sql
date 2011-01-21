
alter table Logs.PaymentLogs
add column PayerInn varchar(255),
add column PayerName varchar(255),
add column PayerAccountCode varchar(255),
add column PayerBankDescription varchar(255),
add column PayerBankBic varchar(255),
add column PayerBankAccountCode varchar(255),
add column RecipientInn varchar(255),
add column RecipientName varchar(255),
add column RecipientAccountCode varchar(255),
add column RecipientBankDescription varchar(255),
add column RecipientBankBic varchar(255),
add column RecipientBankAccountCode varchar(255)
;

DROP TRIGGER IF EXISTS Billing.PaymentLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.PaymentLogDelete AFTER DELETE ON Billing.Payments
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PaymentLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		PaymentId = OLD.Id,
		PayedOn = OLD.PayedOn,
		Sum = OLD.Sum,
		PayerId = OLD.PayerId,
		RecipientId = OLD.RecipientId,
		RegistredOn = OLD.RegistredOn,
		Comment = OLD.Comment,
		DocumentNumber = OLD.DocumentNumber,
		PayerInn = OLD.PayerInn,
		PayerName = OLD.PayerName,
		PayerAccountCode = OLD.PayerAccountCode,
		PayerBankDescription = OLD.PayerBankDescription,
		PayerBankBic = OLD.PayerBankBic,
		PayerBankAccountCode = OLD.PayerBankAccountCode,
		RecipientInn = OLD.RecipientInn,
		RecipientName = OLD.RecipientName,
		RecipientAccountCode = OLD.RecipientAccountCode,
		RecipientBankDescription = OLD.RecipientBankDescription,
		RecipientBankBic = OLD.RecipientBankBic,
		RecipientBankAccountCode = OLD.RecipientBankAccountCode;
END;

DROP TRIGGER IF EXISTS Billing.PaymentLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.PaymentLogUpdate AFTER UPDATE ON Billing.Payments
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PaymentLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		PaymentId = OLD.Id,
		PayedOn = NULLIF(NEW.PayedOn, OLD.PayedOn),
		Sum = NULLIF(NEW.Sum, OLD.Sum),
		PayerId = NULLIF(NEW.PayerId, OLD.PayerId),
		RecipientId = NULLIF(NEW.RecipientId, OLD.RecipientId),
		RegistredOn = NULLIF(NEW.RegistredOn, OLD.RegistredOn),
		Comment = NULLIF(NEW.Comment, OLD.Comment),
		DocumentNumber = NULLIF(NEW.DocumentNumber, OLD.DocumentNumber),
		PayerInn = NULLIF(NEW.PayerInn, OLD.PayerInn),
		PayerName = NULLIF(NEW.PayerName, OLD.PayerName),
		PayerAccountCode = NULLIF(NEW.PayerAccountCode, OLD.PayerAccountCode),
		PayerBankDescription = NULLIF(NEW.PayerBankDescription, OLD.PayerBankDescription),
		PayerBankBic = NULLIF(NEW.PayerBankBic, OLD.PayerBankBic),
		PayerBankAccountCode = NULLIF(NEW.PayerBankAccountCode, OLD.PayerBankAccountCode),
		RecipientInn = NULLIF(NEW.RecipientInn, OLD.RecipientInn),
		RecipientName = NULLIF(NEW.RecipientName, OLD.RecipientName),
		RecipientAccountCode = NULLIF(NEW.RecipientAccountCode, OLD.RecipientAccountCode),
		RecipientBankDescription = NULLIF(NEW.RecipientBankDescription, OLD.RecipientBankDescription),
		RecipientBankBic = NULLIF(NEW.RecipientBankBic, OLD.RecipientBankBic),
		RecipientBankAccountCode = NULLIF(NEW.RecipientBankAccountCode, OLD.RecipientBankAccountCode);
END;

DROP TRIGGER IF EXISTS Billing.PaymentLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.PaymentLogInsert AFTER INSERT ON Billing.Payments
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PaymentLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		PaymentId = NEW.Id,
		PayedOn = NEW.PayedOn,
		Sum = NEW.Sum,
		PayerId = NEW.PayerId,
		RecipientId = NEW.RecipientId,
		RegistredOn = NEW.RegistredOn,
		Comment = NEW.Comment,
		DocumentNumber = NEW.DocumentNumber,
		PayerInn = NEW.PayerInn,
		PayerName = NEW.PayerName,
		PayerAccountCode = NEW.PayerAccountCode,
		PayerBankDescription = NEW.PayerBankDescription,
		PayerBankBic = NEW.PayerBankBic,
		PayerBankAccountCode = NEW.PayerBankAccountCode,
		RecipientInn = NEW.RecipientInn,
		RecipientName = NEW.RecipientName,
		RecipientAccountCode = NEW.RecipientAccountCode,
		RecipientBankDescription = NEW.RecipientBankDescription,
		RecipientBankBic = NEW.RecipientBankBic,
		RecipientBankAccountCode = NEW.RecipientBankAccountCode;
END;

