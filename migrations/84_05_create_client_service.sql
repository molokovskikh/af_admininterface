insert into Future.Services(Id, Type, Name, HomeRegion, Disabled)
select c.Id, 1, c.Name, c.RegionCode, c.Status = 0
from Future.Clients c;

update Future.Users
set RootService = ClientId;
