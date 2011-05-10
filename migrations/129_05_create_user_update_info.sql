create temporary table Usersettings.UsersToInsert
select u.Id as UserId
from Future.Users u
left join Usersettings.UserUpdateInfo uui on uui.UserId = u.Id
where uui.UserId is null;

insert into Usersettings.UserUpdateInfo(UserId)
select UserId from Usersettings.UsersToInsert;
