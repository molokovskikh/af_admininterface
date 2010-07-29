DROP TRIGGER IF EXISTS Future.UserLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.UserLogInsert AFTER INSERT ON Future.Users
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		UserId = NEW.Id,
		ClientId = NEW.ClientId,
		Enabled = NEW.Enabled,
		Login = NEW.Login,
		Name = NEW.Name,
		SendRejects = NEW.SendRejects,
		SendWaybills = NEW.SendWaybills,
		SubmitOrders = NEW.SubmitOrders,
		EnableUpdate = NEW.EnableUpdate,
		Auditor = NEW.Auditor,
		InheritPricesFrom = NEW.InheritPricesFrom,
		Registrant = NEW.Registrant,
		ContactGroupId = NEW.ContactGroupId,
		RegistrationDate = NEW.RegistrationDate,
		WorkRegionMask = NEW.WorkRegionMask,
		OrderRegionMask = NEW.OrderRegionMask,
		Free = NEW.Free,
		AccountingId = NEW.AccountingId;
END;
;
