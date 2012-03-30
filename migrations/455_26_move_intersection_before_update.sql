CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Customers.IntersectionBeforeUpdate BEFORE UPDATE ON Customers.Intersection FOR EACH ROW
BEGIN

	update Usersettings.AnalitFReplicationInfo ar
		join Usersettings.PricesData pd on pd.FirmCode = ar.FirmCode
		join Customers.Users u on ar.UserId = u.Id
			join Customers.Clients c on u.ClientId = c.Id
	set ar.ForceReplication = 1
	where c.Id = OLD.ClientId and pd.PriceCode = OLD.PriceId;

END;
