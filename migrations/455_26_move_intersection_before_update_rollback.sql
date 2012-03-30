drop trigger Future.IntersectionBeforeUpdate;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Future.IntersectionBeforeUpdate BEFORE UPDATE ON Future.Intersection FOR EACH ROW
BEGIN

	update Usersettings.AnalitFReplicationInfo ar
		join Usersettings.PricesData pd on pd.FirmCode = ar.FirmCode
		join Future.Users u on ar.UserId = u.Id
			join Future.Clients c on u.ClientId = c.Id
	set ar.ForceReplication = 1
	where c.Id = OLD.ClientId and pd.PriceCode = OLD.PriceId;

END;
