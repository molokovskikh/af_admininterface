DROP TRIGGER IF EXISTS Customers.ShowUserLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.ShowUserLogDelete AFTER DELETE ON Customers.ShowUsers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.ShowUserLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		PrimaryUserId = OLD.PrimaryUserId,
		ShowUserId = OLD.ShowUserId;
END;

DROP TRIGGER IF EXISTS Customers.ShowUserLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.ShowUserLogUpdate AFTER UPDATE ON Customers.ShowUsers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.ShowUserLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		PrimaryUserId = OLD.PrimaryUserId,
		ShowUserId = OLD.ShowUserId;
END;

DROP TRIGGER IF EXISTS Customers.ShowUserLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Customers.ShowUserLogInsert AFTER INSERT ON Customers.ShowUsers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.ShowUserLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		PrimaryUserId = NEW.PrimaryUserId,
		ShowUserId = NEW.ShowUserId;
END;

