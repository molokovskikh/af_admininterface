create temporary table Usersettings.for_delete engine=memory
select si.Id from Usersettings.SupplierIntersection si
left join Future.Clients c on c.Id = si.ClientId
left join Future.Suppliers s on s.Id = si.SupplierId
where c.Id is null or s.Id is null;

delete si from Usersettings.SupplierIntersection si
join Usersettings.for_delete fd on fd.Id = si.Id
;

alter table Usersettings.SupplierIntersection
add constraint `FK_SupplierIntersection_ClientId` foreign key (ClientId) references Future.Clients(Id) on delete cascade,
add constraint `FK_SupplierIntersection_SupplierId` foreign key (SupplierId) references Future.Suppliers(Id) on delete cascade;
