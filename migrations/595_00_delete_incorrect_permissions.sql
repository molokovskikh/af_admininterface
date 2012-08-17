delete ap from Usersettings.AssignedPermissions ap
join Usersettings.UserPermissions p on p.Id = ap.PermissionId
join Customers.Users u on u.Id = ap.UserId
where u.ClientId is not null and p.AvailableFor = 2;

delete ap from Usersettings.AssignedPermissions ap
join Usersettings.UserPermissions p on p.Id = ap.PermissionId
join Customers.Users u on u.Id = ap.UserId
where u.ClientId is null and p.AvailableFor = 1;
