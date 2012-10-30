
alter table Logs.prices_regional_data_Logs
add column BaseCost int(10) unsigned
;

DROP TRIGGER IF EXISTS usersettings.PricesregionaldataLogDelete;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER usersettings.PricesregionaldatumLogDelete AFTER DELETE ON usersettings.pricesregionaldata
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.prices_regional_data_Logs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		pricesregionaldataID = OLD.RowID,
		PriceCode = OLD.PriceCode,
		RegionCode = OLD.RegionCode,
		UpCost = OLD.UpCost,
		minreq = OLD.MinReq,
		Enabled = OLD.Enabled,
		BaseCost = OLD.BaseCost;
END;

DROP TRIGGER IF EXISTS usersettings.PricesregionaldataLogUpdate;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER usersettings.PricesregionaldatumLogUpdate AFTER UPDATE ON usersettings.pricesregionaldata
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.prices_regional_data_Logs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		pricesregionaldataID = OLD.RowID,
		PriceCode = OLD.PriceCode,
		RegionCode = OLD.RegionCode,
		UpCost = NULLIF(NEW.UpCost, OLD.UpCost),
		minreq = NULLIF(NEW.MinReq, OLD.MinReq),
		Enabled = NULLIF(NEW.Enabled, OLD.Enabled),
		BaseCost = NULLIF(NEW.BaseCost, OLD.BaseCost);
END;

DROP TRIGGER IF EXISTS usersettings.PricesregionaldataLogInsert;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER usersettings.PricesregionaldatumLogInsert AFTER INSERT ON usersettings.pricesregionaldata
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.prices_regional_data_Logs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		pricesregionaldataID = NEW.RowID,
		PriceCode = NEW.PriceCode,
		RegionCode = NEW.RegionCode,
		UpCost = NEW.UpCost,
		minreq = NEW.MinReq,
		Enabled = NEW.Enabled,
		BaseCost = NEW.BaseCost;
END;