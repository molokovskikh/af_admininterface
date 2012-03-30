DROP PROCEDURE Usersettings.GetStatLog;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`GetStatLog`(IN inLogStart DATETIME, IN inLogEnd DATETIME)
BEGIN

  DROP TEMPORARY TABLE IF EXISTS tempdownlogs;
  CREATE TEMPORARY TABLE tempdownlogs(
    rowid INT UNSIGNED,
    priceitemid INT UNSIGNED,
    resultcode MEDIUMINT UNSIGNED,
    fixedtime DATETIME,
    LogTime datetime,
    KEY (rowid),
    KEY (priceitemid),
    KEY (resultcode)
  ) ENGINE = MEMORY;

  DROP TEMPORARY TABLE IF EXISTS tempdownlogs2;
  CREATE TEMPORARY TABLE tempdownlogs2(
    rowid INT UNSIGNED,
    priceitemid INT UNSIGNED,
    resultcode MEDIUMINT UNSIGNED,
    LogTime datetime,
    KEY (rowid),
    KEY (priceitemid),
    KEY (resultcode)
  ) ENGINE = MEMORY;

  INSERT INTO tempdownlogs
  SELECT d.rowid, d.priceitemid, d.resultcode, NULL, d.LogTime
  FROM LOGS.downlogs d
  WHERE (inLogStart < d.logtime) AND (d.logtime < inLogEnd);

  update tempdownlogs t
    join usersettings.PriceItems pi on pi.Id = t.PriceItemId
  SET fixedtime = if (logtime > pi.LastDownload, null, pi.LastDownload)
  where t.ResultCode in (3, 5);
  
  drop temporary table if exists MaxUnfixedErrorTimes;
  create temporary table MaxUnfixedErrorTimes engine = memory
  select PriceItemId, max(logtime) as LogTime
  from tempdownlogs t
  where t.fixedtime is null and t.ResultCode in (3, 5)
  group by t.PriceItemId;
  
  delete t from tempdownlogs t
    join MaxUnfixedErrorTimes m on t.PriceItemId = m.PriceItemId
  where t.fixedtime is null and t.ResultCode in (3, 5) and t.logtime < m.LogTime;
  
  insert into tempdownlogs2
  select d.rowid, d.priceitemid, d.resultcode, max(d.LogTime)
  from LOGS.downlogs d
  where (inLogStart < d.logtime) AND (d.logtime < inLogEnd) and ResultCode = 2
  group by d.priceitemid;
  
  update tempdownlogs t1
    join tempdownlogs2 t2 on t1.PriceItemId = t2.PriceItemId
  set t1.fixedtime = if(t2.logtime > t1.logtime, t2.logtime, null)
  where t1.ResultCode in (3, 5) and t1.fixedtime is null;
  

  DROP TEMPORARY TABLE IF EXISTS tempformlogs;
  CREATE TEMPORARY TABLE tempformlogs(
    rowid INT UNSIGNED PRIMARY KEY,
    logtime DATETIME,
    priceitemid INT UNSIGNED,
    resultid MEDIUMINT UNSIGNED,
    fixedtime DATETIME,
    KEY priceitemid (rowid, priceitemid),
    KEY resultid (rowid, resultid)
  ) ENGINE = MEMORY;
  INSERT
  INTO tempformlogs
  SELECT
    rowid, logtime, priceitemid, resultid, NULL
  FROM
    LOGS.formlogs d
  WHERE
    (inLogStart < logtime) AND (logtime < inLogEnd);
  SELECT
    MIN(Rowid)
  INTO
    @minrowid
  FROM
    tempformlogs;
  UPDATE
    tempformlogs, LOGS.formlogs dl
  SET
    fixedtime = dl.logtime
  WHERE
    tempformlogs.priceitemid = dl.priceitemid
    AND tempformlogs.rowid >= @minrowid
    AND @minrowid < dl.rowid
    AND tempformlogs.rowid < dl.rowid
    AND dl.resultid IN
    (2, 3)
    AND tempformlogs.resultid = 5;
  SELECT
    1 AS LAppCode, 
    LogTime AS LLogTime, 
    logs.priceitemid AS LPriceItemId, 
    NULL AS LFirmName, 
    NULL AS LRegion, 
    NULL AS LPriceName, 
    NULL AS LPriceCode, 
    NULL AS LForm, 
    NULL AS LUnform, 
    NULL AS LZero, 
    NULL AS LForb, 
    IF(ResultCode = 2, NULL, Addition) AS LAddition,
    IF(ResultCode = 2, NULL, IFNULL(ShortErrorMessage, Addition)) AS LShortErrorMessage, 
    srcTypes.Type As LSourceType,
    ResultCode AS LResultID, 1 AS LStatus
  FROM
    LOGS.downlogs AS LOGS
  LEFT JOIN farm.sourcetypes AS srcTypes ON srcTypes.Id = logs.SourceTypeId
  WHERE
    logs.priceitemid IS NULL
    AND logs.LogTime > inLogStart
    AND logs.LogTime < inLogEnd
  UNION
  SELECT
    1 AS LAppCode, 
    logs.LogTime AS LLogTime, 
    logs.priceitemid AS LPriceItemId, 
    s.Name AS LFirmName, 
    r.Region AS LRegion, 
    IF(pd.CostType = 1, CONVERT(CONCAT(pd.PriceName, CONVERT(" [�������] " USING CP1251), pc.CostName) USING CP1251), pd.PriceName) AS LPriceName, 
    pd.PriceCode AS LPriceCode, 
    NULL AS LForm, 
    NULL AS LUnform, 
    NULL AS LZero, 
    NULL AS LForb, 
    IF(logs.ResultCode = 2, NULL, logs.Addition) AS LAddition,
    IF(logs.ResultCode = 2, NULL, IFNULL(ShortErrorMessage, Addition)) AS LShortErrorMessage, 
    srcTypes.Type As LSourceType,
    logs.ResultCode AS LResultID, 
    IF((logs.ResultCode = 2), 0, IF(tempdownlogs.fixedtime IS NULL, 1, 2)) AS LStatus
  FROM
    (LOGS.downlogs AS LOGS,
    usersettings.PriceItems pim,
    Customers.Suppliers s,
    usersettings.pricesdata pd,
    usersettings.pricescosts pc,
    farm.regions r,
    tempdownlogs)
  LEFT JOIN farm.sourcetypes AS srcTypes ON srcTypes.Id = logs.SourceTypeId
  WHERE
    pim.Id = logs.PriceItemId
    AND pc.PriceItemId = pim.Id
    AND pc.PriceCode = pd.PriceCode
    AND ((pd.CostType = 1) OR (pc.BaseCost = 1))
    AND s.Id = pd.firmcode
    AND r.regioncode = s.HomeRegion
    AND tempdownlogs.RowId = logs.RowID
    AND logs.LogTime > inLogStart
    AND logs.LogTime < inLogEnd
  UNION
  SELECT
    0 AS LAppCode, 
    LogTime AS LLogTime, 
    logs.priceitemid AS LPriceItemId, 
    NULL AS LFirmName, 
    NULL AS LRegion, 
    NULL AS LPriceName, 
    NULL AS LPriceCode, 
    IF(Form IS NULL, 0, Form) AS LForm, 
    IF(Unform IS NULL, 0, Unform) AS LUnform, 
    IF(Zero IS NULL, 0, Zero) AS LZero, 
    IF(Forb IS NULL, 0, Forb) AS LForb, 
    Addition AS LAddition, 
    Addition AS LShortErrorMessage, 
    null As LSourceType,
    ResultID AS LResultID, 
    1 AS LStatus
  FROM
    LOGS.formlogs AS LOGS
  WHERE
    logs.PriceItemId IS NULL
    AND logs.LogTime > inLogStart
    AND logs.LogTime < inLogEnd
  UNION
  SELECT
    0 AS LAppCode, 
    logs.LogTime AS LLogTime, 
    logs.priceitemid AS LPriceItemId, 
    s.Name AS LFirmName, 
    r.Region AS LRegion, 
    IF(pd.CostType = 1, CONVERT(CONCAT(pd.PriceName, CONVERT(" [�������] " USING CP1251), pc.CostName) USING CP1251), pd.PriceName) AS LPriceName, 
    pd.PriceCode AS LPriceCode, 
    IF(logs.Form IS NULL, 0, logs.Form) AS LForm, 
    IF(logs.Unform IS NULL, 0, logs.Unform) AS LUnform,
    IF(logs.Zero IS NULL, 0, logs.Zero) AS LZero, 
    IF(logs.Forb IS NULL, 0, logs.Forb) AS LForb, 
    logs.Addition AS LAddition, 
    logs.Addition AS LShortErrorMessage, 
    null As LSourceType,
    logs.ResultID AS LResultID, 
    IF((logs.ResultID = 2) OR (logs.ResultID = 3), 0, IF(tempformlogs.fixedtime IS NULL, 1, 2)) AS LStatus
  FROM
    LOGS.formlogs AS LOGS,
    usersettings.PriceItems pim,
	Customers.Suppliers s,
    usersettings.pricesdata pd,
    usersettings.pricescosts pc,
    farm.regions r,
    tempformlogs
  WHERE
    pim.Id = logs.PriceItemId
    AND pc.PriceItemId = pim.Id
    AND pc.PriceCode = pd.PriceCode
    AND ((pd.CostType = 1) OR (pc.BaseCost = 1))
    AND s.Id = pd.firmcode
    AND r.regioncode = s.HomeRegion
    AND tempformlogs.RowId = logs.RowId
    AND logs.LogTime > inLogStart
    AND logs.LogTime < inLogEnd
  ORDER BY
    LLogTime;
END;
