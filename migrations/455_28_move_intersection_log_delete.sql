CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Customers.IntersectionLogDelete AFTER DELETE ON Customers.Intersection
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.IntersectionLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		IntersectionId = OLD.Id,
		ClientId = OLD.ClientId,
		RegionId = OLD.RegionId,
		PriceId = OLD.PriceId,
		LegalEntityId = OLD.LegalEntityId,
		CostId = OLD.CostId,
		AvailableForClient = OLD.AvailableForClient,
		AgencyEnabled = OLD.AgencyEnabled,
		PriceMarkup = OLD.PriceMarkup,
		SupplierClientId = OLD.SupplierClientId,
		SupplierPaymentId = OLD.SupplierPaymentId;
END;
