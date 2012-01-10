update Future.Users
set WorkRegionMask = 0
where WorkRegionMask is null;

update Future.Users
set OrderRegionMask = 0
where OrderRegionMask is null;
