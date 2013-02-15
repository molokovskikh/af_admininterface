update  `logs`.clientsinfo c join customers.users u on u.id=c.objectid
set c.serviceid = u.rootservice
 where c.Type=2 and u.rootservice <> c.serviceid;

update `logs`.clientsinfo c join customers.addresses u on u.id=c.objectid
set c.serviceid = u.clientid
 where c.Type=3 and u.clientid <> c.serviceid;