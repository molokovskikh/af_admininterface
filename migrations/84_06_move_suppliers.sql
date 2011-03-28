insert into Future.Suppliers(Id, Name, FullName, Disabled, RegionMask, 
	Registrant, RegistrationDate, HomeRegion, ContactGroupOwnerId)
select FirmCode, ShortName, FullName, 0, MaskRegion, Registrant,
	RegistrationDate, RegionCode, ContactGroupOwnerId
from Usersettings.ClientsData
where FirmStatus = 1 and FirmType = 0 and FirmSegment = 0;

insert into Future.Services(Id, Name, HomeRegion)
select Id, Name, HomeRegion
from Future.Suppliers;
