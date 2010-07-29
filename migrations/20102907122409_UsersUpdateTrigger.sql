DROP TRIGGER IF EXISTS Future.UserLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Future.UserLogUpdate AFTER UPDATE ON Future.Users
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.UserLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		UserId = OLD.Id,
		ClientId = NULLIF(NEW.ClientId, OLD.ClientId),
		Enabled = NULLIF(NEW.Enabled, OLD.Enabled),
		Login = NULLIF(NEW.Login, OLD.Login),
		Name = NULLIF(NEW.Name, OLD.Name),
		SendRejects = NULLIF(NEW.SendRejects, OLD.SendRejects),
		SendWaybills = NULLIF(NEW.SendWaybills, OLD.SendWaybills),
		SubmitOrders = NULLIF(NEW.SubmitOrders, OLD.SubmitOrders),
		EnableUpdate = NULLIF(NEW.EnableUpdate, OLD.EnableUpdate),
		Auditor = NULLIF(NEW.Auditor, OLD.Auditor),
		InheritPricesFrom = NULLIF(NEW.InheritPricesFrom, OLD.InheritPricesFrom),
		Registrant = NULLIF(NEW.Registrant, OLD.Registrant),
		ContactGroupId = NULLIF(NEW.ContactGroupId, OLD.ContactGroupId),
		RegistrationDate = NULLIF(NEW.RegistrationDate, OLD.RegistrationDate),
		WorkRegionMask = NULLIF(NEW.WorkRegionMask, OLD.WorkRegionMask),
		OrderRegionMask = NULLIF(NEW.OrderRegionMask, OLD.OrderRegionMask),
		Free = NULLIF(NEW.Free, OLD.Free),
		AccountingId = NULLIF(NEW.AccountingId, OLD.AccountingId);
END;
;
