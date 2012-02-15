create temporary table Usersettings.for_delete engine=memory
select ap.ClientId from Usersettings.CostOptimizationClients ap
left join future.Clients u on u.Id = ap.ClientId
where u.Id is null
;

delete ap from Usersettings.CostOptimizationClients ap
join Usersettings.for_delete fd on fd.ClientId = ap.ClientId;

drop temporary table usersettings.for_delete;

alter table Usersettings.CostOptimizationClients
add constraint `FK_CostOptimizationClients` foreign key (ClientId) references Future.Clients(Id) on delete cascade;
