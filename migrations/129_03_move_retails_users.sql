insert into Future.Users(Id, Enabled, Login, Name, RootService, PayerId, WorkRegionMask, OrderRegionMask)
select ouar.RowId, 1, ouar.OsUserName, cd.FirmCode, cd.ShortName, cd.BillingCode, ouar.RegionMask, ouar.RegionMask
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowContactInfo')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowContactInfo = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowChangePassword')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowChangePassword = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowOrdersManage')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowOrdersManage = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowReports')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowReports = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowConManage')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowConManage = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowClients')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowClients = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowPricesSet')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowPricesSet = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowPricesSources')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowPricesSources = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowDocumentSettings')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowDocumentSettings = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowDocumentLogs')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowDocumentLogs = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowSourceOrders')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowSourceOrders = 1;

insert into usersettings.AssignedPermissions(UserId, PermissionId)
select ouar.RowId,
(select Id from Usersettings.UserPermissions p where p.Shortcut = 'VendorAllowPromotions')
from Usersettings.OsUserAccessRight ouar
join Usersettings.ClientsData cd on cd.FirmCode = ouar.ClientCode
where cd.FirmSegment = 1 and cd.FirmType = 0 and ouar.VendorAllowPromotions = 1;
