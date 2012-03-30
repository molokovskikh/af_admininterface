drop trigger Usersettings.SupplierIntersectionBeforeUpdate;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Usersettings.SupplierIntersectionBeforeUpdate BEFORE UPDATE ON Usersettings.SupplierIntersection FOR EACH ROW
BEGIN

	update Usersettings.AnalitFReplicationInfo ar
		join Customers.Users u on ar.UserId = u.Id
			join Customers.Clients c on u.ClientId = c.Id
	set ar.ForceReplication = 1
	where c.Id = OLD.ClientId and ar.FirmCode = OLD.SupplierId;

END;
