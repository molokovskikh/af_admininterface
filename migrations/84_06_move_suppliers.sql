insert into Future.Suppliers(Id, Name, FullName, Disabled, RegionMask, 
	Registrant, RegistrationDate, HomeRegion, ContactGroupOwnerId)
select FirmCode, ShortName, FullName, FirmStatus = 0, MaskRegion, Registrant,
	RegistrationDate, RegionCode, ContactGroupOwnerId
from Usersettings.ClientsData
where FirmType = 0 and FirmSegment = 0;

insert into Future.Services(Id, Name, HomeRegion, Disabled)
select Id, Name, HomeRegion, Disabled
from Future.Suppliers;
