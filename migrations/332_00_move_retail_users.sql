create temporary table UsersToMigrate engine=memory
select o.RowId as Id
FROM usersettings.clientsdata cd
join usersettings.osuseraccessright o on cd.firmcode = o.clientcode
where
firmsegment = 1 and firmtype = 1
and billingcode <> 921;

create temporary table ClientsToMigrate engine=memory
select cd.FirmCode as Id
FROM usersettings.clientsdata cd
join usersettings.osuseraccessright o on cd.firmcode = o.clientcode
where
firmsegment = 1 and firmtype = 1
and billingcode <> 921
group by cd.FirmCode;

insert into Future.Clients(Id, Status, Segment, FirmType, RegionCode, MaskRegion, Name, FullName, Registrant, RegistrationDate, ContactGroupOwnerId)
SELECT FirmCode, 1, 1, 1, cd.RegionCode, cd.MaskRegion, ShortName, FullName, Registrant, RegistrationDate, ContactGroupOwnerId
FROM usersettings.clientsdata cd
join usersettings.ClientsToMigrate m on cd.firmcode = m.Id;

insert into Future.Services(Id, Type, Name, Disabled, HomeRegion)
SELECT FirmCode, 1, ShortName, 0, cd.RegionCode
FROM usersettings.clientsdata cd
join usersettings.ClientsToMigrate m on cd.firmcode = m.Id;

insert into Billing.PayerClients(PayerId, ClientId)
SELECT BillingCode, FirmCode
FROM usersettings.clientsdata cd
join usersettings.ClientsToMigrate m on cd.firmcode = m.Id;

insert into Future.Users(Id, ClientId, Enabled, Login, Name, WorkRegionMask, PayerId, RootService, RegistrationDate, Registrant)
select ouar.RowId, ouar.ClientCode, 1, ouar.OsUserName, ouar.OsUserName, cd.MaskRegion, cd.BillingCode, ouar.ClientCode, cd.RegistrationDate, cd.Registrant
from Usersettings.OsUserAccessRight ouar
join Usersettings.Clientsdata cd on cd.FirmCode = ouar.ClientCode
join UsersToMigrate m on m.Id = ouar.RowId
;

insert into Billing.Accounts(Type, ObjectId, BeAccounted, ReadyForAccounting)
select 0, Id, 0, 0
from UsersToMigrate;

update Billing.Accounts a
join Future.Users u on u.Id = a.ObjectId and a.Type = 0
join UsersToMigrate m on m.Id = u.Id
set u.AccountingId = a.Id;

update Future.Clients c
join Future.Services ss on ss.Id = c.Id
join ClientsToMigrate m on m.Id = c.Id
join Usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
set c.RegionCode = (
		select (case r.RegionCode
			when 1 then 1099511627776
			when 2 then 2199023255552
			when 4 then 4398046511104
			when 2048 then 8796093022208
			end)
		from Farm.Regions r
		where r.RegionCode & c.RegionCode > 0
),
ss.HomeRegion = (
		select (case r.RegionCode
			when 1 then 1099511627776
			when 2 then 2199023255552
			when 4 then 4398046511104
			when 2048 then 8796093022208
			end)
		from Farm.Regions r
		where r.RegionCode & ss.HomeRegion > 0
),
c.MaskRegion = (
		select sum(case r.RegionCode
			when 1 then 1099511627776
			when 2 then 2199023255552
			when 4 then 4398046511104
			when 2048 then 8796093022208
			end)
		from Farm.Regions r
		where r.RegionCode & c.MaskRegion > 0
),
rcs.WorkRegionMask = (
		select sum(case r.RegionCode
			when 1 then 1099511627776
			when 2 then 2199023255552
			when 4 then 4398046511104
			when 2048 then 8796093022208
			end)
		from Farm.Regions r
		where r.RegionCode & rcs.WorkRegionMask > 0
)
;

update Future.Clients c
join ClientsToMigrate m on m.Id = c.Id
join Future.Intersection i on i.ClientId = c.Id
set i.RegionId = (
		select (case r.RegionCode
			when 1 then 1099511627776
			when 2 then 2199023255552
			when 4 then 4398046511104
			when 2048 then 8796093022208
			end)
		from Farm.Regions r
		where r.RegionCode & i.RegionId > 0
);


update Future.Users u
join UsersToMigrate m on m.Id = u.Id
set 
u.WorkRegionMask = (
		select sum(case r.RegionCode
			when 1 then 1099511627776
			when 2 then 2199023255552
			when 4 then 4398046511104
			when 2048 then 8796093022208
			end)
		from Farm.Regions r
		where r.RegionCode & u.WorkRegionMask > 0
);

INSERT
INTO Future.Intersection (
	ClientId,
	RegionId,
	PriceId,
	LegalEntityId,
	CostId,
	PriceMarkup,
	AgencyEnabled,
	AvailableForClient,
	SupplierClientId,
	SupplierPaymentId
)
SELECT  DISTINCT drugstore.Id,
		regions.regioncode,
		pd.pricecode,
		le.Id,
		ifnull(parent.CostId, (
			SELECT costcode
			FROM pricescosts pcc
			WHERE basecost
				AND pcc.PriceCode = pd.PriceCode
			limit 1
		)),
		ifnull(parent.PriceMarkup, 0),
		ifnull(parent.AgencyEnabled, if(a.IgnoreNewPrices = 1, 0, 1)),
		ifnull(parent.AvailableForClient, if(pd.PriceType = 0, 1, 0)),
		rootIntersection.SupplierClientId,
		rootIntersection.SupplierPaymentId
FROM Future.Clients as drugstore
	join ClientsToMigrate m on m.Id = drugstore.Id
	JOIN retclientsset as a ON a.clientcode = drugstore.Id
	join billing.PayerClients p on p.ClientId = drugstore.Id
		join Billing.LegalEntities le on le.PayerId = p.PayerId
	JOIN future.suppliers s ON s.Segment = drugstore.Segment
		JOIN pricesdata pd ON pd.firmcode = s.Id
	JOIN farm.regions ON (s.RegionMask & regions.regioncode) > 0 and (drugstore.maskregion & regions.regioncode) > 0
		JOIN pricesregionaldata ON pricesregionaldata.pricecode = pd.pricecode AND pricesregionaldata.regioncode = regions.regioncode
	LEFT JOIN Future.Intersection i ON i.PriceId = pd.pricecode and i.RegionId = regions.regioncode and i.ClientId = drugstore.Id and i.LegalEntityId = le.Id
	LEFT JOIN Future.Intersection parent ON parent.PriceId = pd.pricecode and parent.RegionId = regions.regioncode and parent.ClientId = drugstore.Id
	LEFT JOIN pricesdata as rootPrice on rootPrice.PriceCode = (select min(pricecode) from pricesdata as p where p.firmcode = s.Id)
		LEFT JOIN future.intersection as rootIntersection on rootIntersection.PriceId = rootPrice.PriceCode and rootIntersection.RegionId = Regions.RegionCode and rootIntersection.ClientId = drugstore.Id
			and rootIntersection.LegalEntityId = le.Id
WHERE i.Id IS NULL
group by pd.pricecode, regions.regioncode, drugstore.Id, le.Id;

delete up from Future.UserPrices up
join UsersToMigrate m on m.Id = up.UserId
join Future.Users u on u.Id = up.UserId
join Usersettings.Intersection i on (case i.RegionCode
		when 1 then 1099511627776
		when 2 then 2199023255552
		when 4 then 4398046511104
		when 2048 then 8796093022208
	end) = up.RegionId and i.ClientCode = u.ClientId and i.PriceCode = up.PriceId
where i.DisabledByClient = 1;