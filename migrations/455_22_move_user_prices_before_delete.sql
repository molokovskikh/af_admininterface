CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Customers.`UserPricesBeforeDelete`
BEFORE DELETE ON Customers.`userprices`
FOR EACH ROW
BEGIN

	update 
		Usersettings.AnalitFReplicationInfo ar
		join Usersettings.PricesData pd on pd.FirmCode = ar.FirmCode
	set ForceReplication = 1
	where ar.UserId = OLD.UserId and pd.PriceCode = OLD.PriceId;

	update 
		Customers.Users u
		join Usersettings.AnalitFReplicationInfo ar on ar.UserId = u.Id
		join Usersettings.PricesData pd on pd.FirmCode = ar.FirmCode and pd.PriceCode = OLD.PriceId
	set ForceReplication = 1
	where 
		u.InheritPricesFrom = OLD.UserId;
END;
