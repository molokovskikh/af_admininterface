drop trigger future.SupplierLogInsert;

drop temporary table if exists for_insert;
create temporary table for_insert engine=memory
select *
from Usersettings.ClientsData
where FirmType = 0 and FirmSegment = 1;

insert into Future.Suppliers(Id, Name, FullName, Disabled, RegionMask, 
	Registrant, RegistrationDate, HomeRegion, ContactGroupOwnerId, Payer, Segment)
select FirmCode, ShortName, FullName, FirmStatus = 0, MaskRegion, Registrant,
	RegistrationDate, RegionCode, ContactGroupOwnerId, BillingCode, FirmSegment
from for_insert
;

insert into Future.Services(Id, Type, Name, HomeRegion, Disabled)
select Id, 0, Name, HomeRegion, Disabled
from Future.Suppliers
where Segment = 1;

