
alter table Logs.DefaultLogs
add column Phones varchar(255)
;

DROP TRIGGER IF EXISTS Usersettings.DefaultLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.DefaultLogDelete AFTER DELETE ON Usersettings.Defaults
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.DefaultLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		AnalitFVersion = OLD.AnalitFVersion,
		DefaultId = OLD.Id,
		FormaterId = OLD.FormaterId,
		SenderId = OLD.SenderId,
		EmailFooter = OLD.EmailFooter,
		Phones = OLD.Phones;
END;

DROP TRIGGER IF EXISTS Usersettings.DefaultLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.DefaultLogUpdate AFTER UPDATE ON Usersettings.Defaults
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.DefaultLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		AnalitFVersion = NULLIF(NEW.AnalitFVersion, OLD.AnalitFVersion),
		DefaultId = OLD.Id,
		FormaterId = NULLIF(NEW.FormaterId, OLD.FormaterId),
		SenderId = NULLIF(NEW.SenderId, OLD.SenderId),
		EmailFooter = NULLIF(NEW.EmailFooter, OLD.EmailFooter),
		Phones = NULLIF(NEW.Phones, OLD.Phones);
END;

DROP TRIGGER IF EXISTS Usersettings.DefaultLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.DefaultLogInsert AFTER INSERT ON Usersettings.Defaults
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.DefaultLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		AnalitFVersion = NEW.AnalitFVersion,
		DefaultId = NEW.Id,
		FormaterId = NEW.FormaterId,
		SenderId = NEW.SenderId,
		EmailFooter = NEW.EmailFooter,
		Phones = NEW.Phones;
END;

