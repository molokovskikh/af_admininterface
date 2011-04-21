create temporary table Usersettings.UsersToInsert
select u.Id as UserId
from Future.Users u
left join logs.AuthorizationDates uui on uui.UserId = u.Id
where uui.UserId is null;

insert into logs.AuthorizationDates(UserId)
select UserId from Usersettings.UsersToInsert;
