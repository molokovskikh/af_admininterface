DROP TRIGGER IF EXISTS Customers.UserAddressLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.UserAddressLogDelete AFTER DELETE ON Customers.UserAddresses
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserAddressLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		UserId = OLD.UserId,
		AddressId = OLD.AddressId;
END;

DROP TRIGGER IF EXISTS Customers.UserAddressLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.UserAddressLogUpdate AFTER UPDATE ON Customers.UserAddresses
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserAddressLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		UserId = OLD.UserId,
		AddressId = OLD.AddressId;
END;

DROP TRIGGER IF EXISTS Customers.UserAddressLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.UserAddressLogInsert AFTER INSERT ON Customers.UserAddresses
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserAddressLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		UserId = NEW.UserId,
		AddressId = NEW.AddressId;
END;

