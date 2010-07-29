DROP TRIGGER IF EXISTS Future.UserLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.UserLogDelete AFTER DELETE ON Future.Users
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		UserId = OLD.Id,
		ClientId = OLD.ClientId,
		Enabled = OLD.Enabled,
		Login = OLD.Login,
		Name = OLD.Name,
		SendRejects = OLD.SendRejects,
		SendWaybills = OLD.SendWaybills,
		SubmitOrders = OLD.SubmitOrders,
		EnableUpdate = OLD.EnableUpdate,
		Auditor = OLD.Auditor,
		InheritPricesFrom = OLD.InheritPricesFrom,
		Registrant = OLD.Registrant,
		ContactGroupId = OLD.ContactGroupId,
		RegistrationDate = OLD.RegistrationDate,
		WorkRegionMask = OLD.WorkRegionMask,
		OrderRegionMask = OLD.OrderRegionMask,
		Free = OLD.Free,
		AccountingId = OLD.AccountingId;
END;
;
