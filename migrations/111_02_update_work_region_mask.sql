update Future.Users u
join Usersettings.osuseraccessright ouar on u.Login = ouar.OsUserName
set u.OrderRegionMask = ouar.RegionMask, u.WorkRegionMask = ouar.RegionMask
where u.OrderRegionMask is null
