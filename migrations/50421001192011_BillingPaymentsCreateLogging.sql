
CREATE TABLE  `logs`.`PaymentLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `PaymentId` int(10) unsigned not null,
  `PayedOn` datetime,
  `Sum` decimal(10,2),
  `PayerId` int(10) unsigned,
  `PaymentType` tinyint(1) unsigned,
  `Name` varchar(255),
  `Discount` int(10) unsigned,
  `RecipientId` int(10) unsigned,
  `RegistredOn` datetime,
  `BankPaymentId` varchar(255),
  `Comment` varchar(255),
  `DocumentNumber` varchar(255),

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

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
		PaymentType = OLD.PaymentType,
		Name = OLD.Name,
		Discount = OLD.Discount,
		RecipientId = OLD.RecipientId,
		RegistredOn = OLD.RegistredOn,
		BankPaymentId = OLD.BankPaymentId,
		Comment = OLD.Comment,
		DocumentNumber = OLD.DocumentNumber;
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
		PaymentType = NULLIF(NEW.PaymentType, OLD.PaymentType),
		Name = NULLIF(NEW.Name, OLD.Name),
		Discount = NULLIF(NEW.Discount, OLD.Discount),
		RecipientId = NULLIF(NEW.RecipientId, OLD.RecipientId),
		RegistredOn = NULLIF(NEW.RegistredOn, OLD.RegistredOn),
		BankPaymentId = NULLIF(NEW.BankPaymentId, OLD.BankPaymentId),
		Comment = NULLIF(NEW.Comment, OLD.Comment),
		DocumentNumber = NULLIF(NEW.DocumentNumber, OLD.DocumentNumber);
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
		PaymentType = NEW.PaymentType,
		Name = NEW.Name,
		Discount = NEW.Discount,
		RecipientId = NEW.RecipientId,
		RegistredOn = NEW.RegistredOn,
		BankPaymentId = NEW.BankPaymentId,
		Comment = NEW.Comment,
		DocumentNumber = NEW.DocumentNumber;
END;

