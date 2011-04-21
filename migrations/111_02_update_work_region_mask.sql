update Future.Users u
join Usersettings.osuseraccessright ouar
set u.OrderRegionMask = ouar.RegionMask, u.WorkRegionMask = ouar.RegionMask
where u.OrderRegionMask is null
