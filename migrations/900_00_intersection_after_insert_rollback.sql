drop trigger customers.IntersectionAfterInsert;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Customers.IntersectionAfterInsert
	AFTER INSERT
	ON Customers.Intersection
	FOR EACH ROW
BEGIN
  IF @Skip IS NULL OR @Skip <> 1 THEN

    INSERT
    INTO Customers.AddressIntersection (IntersectionId, AddressId)
    SELECT
      NEW.Id, a.Id
    FROM
      Customers.Clients c
      JOIN Customers.Addresses a ON a.ClientId = c.Id
    WHERE
      c.Id = NEW.ClientId and a.LegalEntityId = NEW.LegalEntityId;

    INSERT
    INTO Customers.UserPrices (UserId, PriceId, RegionId)
    SELECT
      u.Id, NEW.PriceId, NEW.RegionId
    FROM
		Customers.Clients c
		join Customers.Users u ON u.ClientId = c.Id
		join Usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
		left join Customers.UserPrices up ON up.PriceId = NEW.PriceId
			AND up.RegionId = NEW.RegionId AND up.UserId = u.Id
    WHERE
      c.Id = NEW.ClientId AND up.UserId IS NULL and rcs.IgnoreNewPriceForUser = 0;

    INSERT
    INTO Usersettings.SupplierIntersection (SupplierId, ClientId)
    SELECT
      pd.FirmCode, NEW.ClientId
    FROM
      Usersettings.PricesData pd
      LEFT JOIN Usersettings.SupplierIntersection si ON si.SupplierId = pd.FirmCode AND si.ClientId = NEW.ClientId
    WHERE
      pd.PriceCode = NEW.PriceId AND si.Id IS NULL
    GROUP BY
      pd.FirmCode;

    insert into
      usersettings.PriceIntersections (SupplierIntersectionId, PriceId)
    SELECT
      si.Id, pd.PriceCode
    FROM
      usersettings.PricesData pd
      inner join usersettings.SupplierIntersection si on si.SupplierId = pd.FirmCode
      left join usersettings.PriceIntersections pi on pi.SupplierIntersectionId = si.Id and pi.PriceId = pd.PriceCode
    where
      pd.PriceCode = NEW.PriceId
      and si.ClientId = NEW.ClientId
      and pi.Id is null;

    insert into
      usersettings.DelayOfPayments (PriceIntersectionId, DayOfWeek)
    SELECT
      pi.Id, d.DayOfWeek
    FROM
      (
      usersettings.PriceIntersections pi,
      usersettings.SupplierIntersection si,
    (
    select 'Monday' as DayOfWeek
    union
    select 'Tuesday'
    union
    select 'Wednesday'
    union
    select 'Thursday'
    union
    select 'Friday'
    union
    select 'Saturday'
    union
    select 'Sunday'
    ) d
      )
      left join usersettings.DelayOfPayments dop on dop.PriceIntersectionId = pi.Id and dop.DayOfWeek = convert(d.DayOfWeek using cp1251)
    where
      pi.PriceId = NEW.PriceId
      and si.Id = pi.SupplierIntersectionId
      and si.ClientId = NEW.ClientId
      and dop.Id is null;

  END IF;

  INSERT
  INTO `logs`.IntersectionLogs
  SET
    LogTime = NOW(),
	OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(), '@', 1)),
	OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(), '@', -1)),
	Operation = 0,
	IntersectionId = NEW.Id,
	ClientId = NEW.ClientId,
	RegionId = NEW.RegionId,
	PriceId = NEW.PriceId,
	LegalEntityId = NEW.LegalEntityId,
	CostId = NEW.CostId,
	AvailableForClient = NEW.AvailableForClient,
	AgencyEnabled = NEW.AgencyEnabled,
	PriceMarkup = NEW.PriceMarkup,
	SupplierClientId = NEW.SupplierClientId,
	SupplierPaymentId = NEW.SupplierPaymentId;
END;
