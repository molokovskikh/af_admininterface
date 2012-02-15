create temporary table Usersettings.for_delete engine=memory
select ap.UserId from usersettings.assignedpermissions ap
left join future.Users u on u.Id = ap.UserId
where u.Id is null
;

delete ap from usersettings.assignedpermissions ap
join Usersettings.for_delete fd on fd.UserId = ap.UserId;

drop temporary table usersettings.for_delete;

create temporary table Usersettings.for_delete engine=memory
select ap.UserId from Usersettings.AnalitFReplicationInfo ap
left join future.Users u on u.Id = ap.UserId
where u.Id is null
;

delete ap from Usersettings.AnalitFReplicationInfo ap
join Usersettings.for_delete fd on fd.UserId = ap.UserId;

drop temporary table usersettings.for_delete;


alter table Usersettings.AssignedPermissions
add constraint `FK_AssignedPermission_UserId` foreign key (UserId) references Future.Users(Id) on delete cascade;

alter table Usersettings.AnalitFReplicationInfo
add constraint `FK_AnalitFReplicationInfo_UserId` foreign key (UserId) references Future.Users(Id) on delete cascade;

alter table Logs.AnalitFUpdates
add constraint `FK_AnalitFUpdates_UserId` foreign key (UserId) references Future.Users(Id) on delete cascade;
