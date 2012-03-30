DROP TRIGGER IF EXISTS Customers.UserPriceLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.UserPriceLogDelete AFTER DELETE ON Customers.UserPrices
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserPriceLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		UserId = OLD.UserId,
		PriceId = OLD.PriceId,
		RegionId = OLD.RegionId;
END;

DROP TRIGGER IF EXISTS Customers.UserPriceLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.UserPriceLogUpdate AFTER UPDATE ON Customers.UserPrices
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserPriceLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		UserId = OLD.UserId,
		PriceId = OLD.PriceId,
		RegionId = OLD.RegionId;
END;

DROP TRIGGER IF EXISTS Customers.UserPriceLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.UserPriceLogInsert AFTER INSERT ON Customers.UserPrices
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserPriceLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		UserId = NEW.UserId,
		PriceId = NEW.PriceId,
		RegionId = NEW.RegionId;
END;

