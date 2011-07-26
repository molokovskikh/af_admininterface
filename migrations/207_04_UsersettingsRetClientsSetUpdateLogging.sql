
alter table Logs.RetClientsSetLogs
add column IgnoreNewPriceForUser tinyint(1)
;

DROP TRIGGER IF EXISTS Usersettings.RetClientsSetLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.RetClientsSetLogDelete AFTER DELETE ON Usersettings.RetClientsSet
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.RetClientsSetLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		ClientCode = OLD.ClientCode,
		InvisibleOnFirm = OLD.InvisibleOnFirm,
		BaseFirmCategory = OLD.BaseFirmCategory,
		RetUpCost = OLD.RetUpCost,
		OverCostPercent = OLD.OverCostPercent,
		DifferenceCalculation = OLD.DifferenceCalculation,
		AlowRegister = OLD.AlowRegister,
		AlowRejection = OLD.AlowRejection,
		AlowDocuments = OLD.AlowDocuments,
		MultiUserLevel = OLD.MultiUserLevel,
		AdvertisingLevel = OLD.AdvertisingLevel,
		AlowWayBill = OLD.AlowWayBill,
		AllowDocuments = OLD.AllowDocuments,
		AlowChangeSegment = OLD.AlowChangeSegment,
		ShowPriceName = OLD.ShowPriceName,
		WorkRegionMask = OLD.WorkRegionMask,
		OrderRegionMask = OLD.OrderRegionMask,
		EnableUpdate = OLD.EnableUpdate,
		CheckCopyID = OLD.CheckCopyID,
		AlowCumulativeUpdate = OLD.AlowCumulativeUpdate,
		CheckCumulativeUpdateStatus = OLD.CheckCumulativeUpdateStatus,
		ServiceClient = OLD.ServiceClient,
		SubmitOrders = OLD.SubmitOrders,
		AllowSubmitOrders = OLD.AllowSubmitOrders,
		BasecostPassword = OLD.BasecostPassword,
		OrdersVisualizationMode = OLD.OrdersVisualizationMode,
		CalculateLeader = OLD.CalculateLeader,
		AllowPreparatInfo = OLD.AllowPreparatInfo,
		AllowPreparatDesc = OLD.AllowPreparatDesc,
		SmartOrderRuleId = OLD.SmartOrderRuleId,
		FirmCodeOnly = OLD.FirmCodeOnly,
		MaxWeeklyOrdersSum = OLD.MaxWeeklyOrdersSum,
		CheckWeeklyOrdersSum = OLD.CheckWeeklyOrdersSum,
		AllowDelayOfPayment = OLD.AllowDelayOfPayment,
		Spy = OLD.Spy,
		SpyAccount = OLD.SpyAccount,
		ShowNewDefecture = OLD.ShowNewDefecture,
		MigrateToPrgDataService = OLD.MigrateToPrgDataService,
		ManualComparison = OLD.ManualComparison,
		ParseWaybills = OLD.ParseWaybills,
		SendRetailMarkup = OLD.SendRetailMarkup,
		ShowAdvertising = OLD.ShowAdvertising,
		IgnoreNewPrices = OLD.IgnoreNewPrices,
		SendWaybillsFromClient = OLD.SendWaybillsFromClient,
		OnlyParseWaybills = OLD.OnlyParseWaybills,
		UpdateToTestBuild = OLD.UpdateToTestBuild,
		EnableSmartOrder = OLD.EnableSmartOrder,
		BuyingMatrixPriceId = OLD.BuyingMatrixPriceId,
		BuyingMatrixType = OLD.BuyingMatrixType,
		WarningOnBuyingMatrix = OLD.WarningOnBuyingMatrix,
		EnableImpersonalPrice = OLD.EnableImpersonalPrice,
		NetworkSupplierId = OLD.NetworkSupplierId,
		NoiseCosts = OLD.NoiseCosts,
		NoiseCostExceptSupplierId = OLD.NoiseCostExceptSupplierId,
		NetworkPriceId = OLD.NetworkPriceId,
		IsConvertFormat = OLD.IsConvertFormat,
		AssortimentPriceId = OLD.AssortimentPriceId,
		OfferMatrixPriceId = OLD.OfferMatrixPriceId,
		OfferMatrixType = OLD.OfferMatrixType,
		IgnoreNewPriceForUser = OLD.IgnoreNewPriceForUser;
END;

DROP TRIGGER IF EXISTS Usersettings.RetClientsSetLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.RetClientsSetLogUpdate AFTER UPDATE ON Usersettings.RetClientsSet
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.RetClientsSetLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		ClientCode = OLD.ClientCode,
		InvisibleOnFirm = NULLIF(NEW.InvisibleOnFirm, OLD.InvisibleOnFirm),
		BaseFirmCategory = NULLIF(NEW.BaseFirmCategory, OLD.BaseFirmCategory),
		RetUpCost = NULLIF(NEW.RetUpCost, OLD.RetUpCost),
		OverCostPercent = NULLIF(NEW.OverCostPercent, OLD.OverCostPercent),
		DifferenceCalculation = NULLIF(NEW.DifferenceCalculation, OLD.DifferenceCalculation),
		AlowRegister = NULLIF(NEW.AlowRegister, OLD.AlowRegister),
		AlowRejection = NULLIF(NEW.AlowRejection, OLD.AlowRejection),
		AlowDocuments = NULLIF(NEW.AlowDocuments, OLD.AlowDocuments),
		MultiUserLevel = NULLIF(NEW.MultiUserLevel, OLD.MultiUserLevel),
		AdvertisingLevel = NULLIF(NEW.AdvertisingLevel, OLD.AdvertisingLevel),
		AlowWayBill = NULLIF(NEW.AlowWayBill, OLD.AlowWayBill),
		AllowDocuments = NULLIF(NEW.AllowDocuments, OLD.AllowDocuments),
		AlowChangeSegment = NULLIF(NEW.AlowChangeSegment, OLD.AlowChangeSegment),
		ShowPriceName = NULLIF(NEW.ShowPriceName, OLD.ShowPriceName),
		WorkRegionMask = NULLIF(NEW.WorkRegionMask, OLD.WorkRegionMask),
		OrderRegionMask = NULLIF(NEW.OrderRegionMask, OLD.OrderRegionMask),
		EnableUpdate = NULLIF(NEW.EnableUpdate, OLD.EnableUpdate),
		CheckCopyID = NULLIF(NEW.CheckCopyID, OLD.CheckCopyID),
		AlowCumulativeUpdate = NULLIF(NEW.AlowCumulativeUpdate, OLD.AlowCumulativeUpdate),
		CheckCumulativeUpdateStatus = NULLIF(NEW.CheckCumulativeUpdateStatus, OLD.CheckCumulativeUpdateStatus),
		ServiceClient = NULLIF(NEW.ServiceClient, OLD.ServiceClient),
		SubmitOrders = NULLIF(NEW.SubmitOrders, OLD.SubmitOrders),
		AllowSubmitOrders = NULLIF(NEW.AllowSubmitOrders, OLD.AllowSubmitOrders),
		BasecostPassword = NULLIF(NEW.BasecostPassword, OLD.BasecostPassword),
		OrdersVisualizationMode = NULLIF(NEW.OrdersVisualizationMode, OLD.OrdersVisualizationMode),
		CalculateLeader = NULLIF(NEW.CalculateLeader, OLD.CalculateLeader),
		AllowPreparatInfo = NULLIF(NEW.AllowPreparatInfo, OLD.AllowPreparatInfo),
		AllowPreparatDesc = NULLIF(NEW.AllowPreparatDesc, OLD.AllowPreparatDesc),
		SmartOrderRuleId = NULLIF(NEW.SmartOrderRuleId, OLD.SmartOrderRuleId),
		FirmCodeOnly = NULLIF(NEW.FirmCodeOnly, OLD.FirmCodeOnly),
		MaxWeeklyOrdersSum = NULLIF(NEW.MaxWeeklyOrdersSum, OLD.MaxWeeklyOrdersSum),
		CheckWeeklyOrdersSum = NULLIF(NEW.CheckWeeklyOrdersSum, OLD.CheckWeeklyOrdersSum),
		AllowDelayOfPayment = NULLIF(NEW.AllowDelayOfPayment, OLD.AllowDelayOfPayment),
		Spy = NULLIF(NEW.Spy, OLD.Spy),
		SpyAccount = NULLIF(NEW.SpyAccount, OLD.SpyAccount),
		ShowNewDefecture = NULLIF(NEW.ShowNewDefecture, OLD.ShowNewDefecture),
		MigrateToPrgDataService = NULLIF(NEW.MigrateToPrgDataService, OLD.MigrateToPrgDataService),
		ManualComparison = NULLIF(NEW.ManualComparison, OLD.ManualComparison),
		ParseWaybills = NULLIF(NEW.ParseWaybills, OLD.ParseWaybills),
		SendRetailMarkup = NULLIF(NEW.SendRetailMarkup, OLD.SendRetailMarkup),
		ShowAdvertising = NULLIF(NEW.ShowAdvertising, OLD.ShowAdvertising),
		IgnoreNewPrices = NULLIF(NEW.IgnoreNewPrices, OLD.IgnoreNewPrices),
		SendWaybillsFromClient = NULLIF(NEW.SendWaybillsFromClient, OLD.SendWaybillsFromClient),
		OnlyParseWaybills = NULLIF(NEW.OnlyParseWaybills, OLD.OnlyParseWaybills),
		UpdateToTestBuild = NULLIF(NEW.UpdateToTestBuild, OLD.UpdateToTestBuild),
		EnableSmartOrder = NULLIF(NEW.EnableSmartOrder, OLD.EnableSmartOrder),
		BuyingMatrixPriceId = NULLIF(NEW.BuyingMatrixPriceId, OLD.BuyingMatrixPriceId),
		BuyingMatrixType = NULLIF(NEW.BuyingMatrixType, OLD.BuyingMatrixType),
		WarningOnBuyingMatrix = NULLIF(NEW.WarningOnBuyingMatrix, OLD.WarningOnBuyingMatrix),
		EnableImpersonalPrice = NULLIF(NEW.EnableImpersonalPrice, OLD.EnableImpersonalPrice),
		NetworkSupplierId = NULLIF(NEW.NetworkSupplierId, OLD.NetworkSupplierId),
		NoiseCosts = NULLIF(NEW.NoiseCosts, OLD.NoiseCosts),
		NoiseCostExceptSupplierId = NULLIF(NEW.NoiseCostExceptSupplierId, OLD.NoiseCostExceptSupplierId),
		NetworkPriceId = NULLIF(NEW.NetworkPriceId, OLD.NetworkPriceId),
		IsConvertFormat = NULLIF(NEW.IsConvertFormat, OLD.IsConvertFormat),
		AssortimentPriceId = NULLIF(NEW.AssortimentPriceId, OLD.AssortimentPriceId),
		OfferMatrixPriceId = NULLIF(NEW.OfferMatrixPriceId, OLD.OfferMatrixPriceId),
		OfferMatrixType = NULLIF(NEW.OfferMatrixType, OLD.OfferMatrixType),
		IgnoreNewPriceForUser = NULLIF(NEW.IgnoreNewPriceForUser, OLD.IgnoreNewPriceForUser);
END;

DROP TRIGGER IF EXISTS Usersettings.RetClientsSetLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Usersettings.RetClientsSetLogInsert AFTER INSERT ON Usersettings.RetClientsSet
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.RetClientsSetLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		ClientCode = NEW.ClientCode,
		InvisibleOnFirm = NEW.InvisibleOnFirm,
		BaseFirmCategory = NEW.BaseFirmCategory,
		RetUpCost = NEW.RetUpCost,
		OverCostPercent = NEW.OverCostPercent,
		DifferenceCalculation = NEW.DifferenceCalculation,
		AlowRegister = NEW.AlowRegister,
		AlowRejection = NEW.AlowRejection,
		AlowDocuments = NEW.AlowDocuments,
		MultiUserLevel = NEW.MultiUserLevel,
		AdvertisingLevel = NEW.AdvertisingLevel,
		AlowWayBill = NEW.AlowWayBill,
		AllowDocuments = NEW.AllowDocuments,
		AlowChangeSegment = NEW.AlowChangeSegment,
		ShowPriceName = NEW.ShowPriceName,
		WorkRegionMask = NEW.WorkRegionMask,
		OrderRegionMask = NEW.OrderRegionMask,
		EnableUpdate = NEW.EnableUpdate,
		CheckCopyID = NEW.CheckCopyID,
		AlowCumulativeUpdate = NEW.AlowCumulativeUpdate,
		CheckCumulativeUpdateStatus = NEW.CheckCumulativeUpdateStatus,
		ServiceClient = NEW.ServiceClient,
		SubmitOrders = NEW.SubmitOrders,
		AllowSubmitOrders = NEW.AllowSubmitOrders,
		BasecostPassword = NEW.BasecostPassword,
		OrdersVisualizationMode = NEW.OrdersVisualizationMode,
		CalculateLeader = NEW.CalculateLeader,
		AllowPreparatInfo = NEW.AllowPreparatInfo,
		AllowPreparatDesc = NEW.AllowPreparatDesc,
		SmartOrderRuleId = NEW.SmartOrderRuleId,
		FirmCodeOnly = NEW.FirmCodeOnly,
		MaxWeeklyOrdersSum = NEW.MaxWeeklyOrdersSum,
		CheckWeeklyOrdersSum = NEW.CheckWeeklyOrdersSum,
		AllowDelayOfPayment = NEW.AllowDelayOfPayment,
		Spy = NEW.Spy,
		SpyAccount = NEW.SpyAccount,
		ShowNewDefecture = NEW.ShowNewDefecture,
		MigrateToPrgDataService = NEW.MigrateToPrgDataService,
		ManualComparison = NEW.ManualComparison,
		ParseWaybills = NEW.ParseWaybills,
		SendRetailMarkup = NEW.SendRetailMarkup,
		ShowAdvertising = NEW.ShowAdvertising,
		IgnoreNewPrices = NEW.IgnoreNewPrices,
		SendWaybillsFromClient = NEW.SendWaybillsFromClient,
		OnlyParseWaybills = NEW.OnlyParseWaybills,
		UpdateToTestBuild = NEW.UpdateToTestBuild,
		EnableSmartOrder = NEW.EnableSmartOrder,
		BuyingMatrixPriceId = NEW.BuyingMatrixPriceId,
		BuyingMatrixType = NEW.BuyingMatrixType,
		WarningOnBuyingMatrix = NEW.WarningOnBuyingMatrix,
		EnableImpersonalPrice = NEW.EnableImpersonalPrice,
		NetworkSupplierId = NEW.NetworkSupplierId,
		NoiseCosts = NEW.NoiseCosts,
		NoiseCostExceptSupplierId = NEW.NoiseCostExceptSupplierId,
		NetworkPriceId = NEW.NetworkPriceId,
		IsConvertFormat = NEW.IsConvertFormat,
		AssortimentPriceId = NEW.AssortimentPriceId,
		OfferMatrixPriceId = NEW.OfferMatrixPriceId,
		OfferMatrixType = NEW.OfferMatrixType,
		IgnoreNewPriceForUser = NEW.IgnoreNewPriceForUser;
END;

