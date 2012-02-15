alter table Usersettings.SupplierIntersection
change column ClientId ClientId int unsigned not null,
change column SupplierId SupplierId int unsigned not null;
