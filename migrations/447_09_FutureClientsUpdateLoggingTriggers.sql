DROP TRIGGER IF EXISTS Future.ClientLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.ClientLogDelete AFTER DELETE ON Future.Clients
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.ClientLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		ClientId = OLD.Id,
		Status = OLD.Status,
		FirmType = OLD.FirmType,
		PayerId = OLD.PayerId,
		RegionCode = OLD.RegionCode,
		MaskRegion = OLD.MaskRegion,
		Name = OLD.Name,
		FullName = OLD.FullName,
		Registrant = OLD.Registrant,
		RegistrationDate = OLD.RegistrationDate,
		ContactGroupOwnerId = OLD.ContactGroupOwnerId,
		SwapFirmCode = OLD.SwapFirmCode;
END;

DROP TRIGGER IF EXISTS Future.ClientLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.ClientLogUpdate AFTER UPDATE ON Future.Clients
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.ClientLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		ClientId = OLD.Id,
		Status = NULLIF(NEW.Status, OLD.Status),
		FirmType = NULLIF(NEW.FirmType, OLD.FirmType),
		PayerId = NULLIF(NEW.PayerId, OLD.PayerId),
		RegionCode = NULLIF(NEW.RegionCode, OLD.RegionCode),
		MaskRegion = NULLIF(NEW.MaskRegion, OLD.MaskRegion),
		Name = NULLIF(NEW.Name, OLD.Name),
		FullName = NULLIF(NEW.FullName, OLD.FullName),
		Registrant = NULLIF(NEW.Registrant, OLD.Registrant),
		RegistrationDate = NULLIF(NEW.RegistrationDate, OLD.RegistrationDate),
		ContactGroupOwnerId = NULLIF(NEW.ContactGroupOwnerId, OLD.ContactGroupOwnerId),
		SwapFirmCode = NULLIF(NEW.SwapFirmCode, OLD.SwapFirmCode);
END;

DROP TRIGGER IF EXISTS Future.ClientLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.ClientLogInsert AFTER INSERT ON Future.Clients
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.ClientLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		ClientId = NEW.Id,
		Status = NEW.Status,
		FirmType = NEW.FirmType,
		PayerId = NEW.PayerId,
		RegionCode = NEW.RegionCode,
		MaskRegion = NEW.MaskRegion,
		Name = NEW.Name,
		FullName = NEW.FullName,
		Registrant = NEW.Registrant,
		RegistrationDate = NEW.RegistrationDate,
		ContactGroupOwnerId = NEW.ContactGroupOwnerId,
		SwapFirmCode = NEW.SwapFirmCode;
END;

