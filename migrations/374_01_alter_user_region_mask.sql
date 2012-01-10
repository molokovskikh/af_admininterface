alter table Future.Users
change column WorkRegionMask WorkRegionMask bigint unsigned not null default 0,
change column OrderRegionMask OrderRegionMask bigint unsigned not null default 0;
