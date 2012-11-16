
CREATE TABLE  `logs`.`ActLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `ActId` int(10) unsigned not null,
  `Period` char(8),
  `ActDate` datetime,
  `Recipient` int(10) unsigned,
  `Payer` int(10) unsigned,
  `Sum` decimal(19,5),
  `PayerName` varchar(255),
  `Customer` varchar(255),

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

DROP TRIGGER IF EXISTS Billing.ActLogDelete;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.ActLogDelete AFTER DELETE ON Billing.Acts
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
		Customer = OLD.Customer;
END;

DROP TRIGGER IF EXISTS Billing.ActLogUpdate;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.ActLogUpdate AFTER UPDATE ON Billing.Acts
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
		Customer = NULLIF(NEW.Customer, OLD.Customer);
END;

DROP TRIGGER IF EXISTS Billing.ActLogInsert;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.ActLogInsert AFTER INSERT ON Billing.Acts
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
		Customer = NEW.Customer;
END;
