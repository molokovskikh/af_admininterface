delete p from Usersettings.RegionalData p
left join Future.Suppliers s on s.Id = p.FirmCode
where s.Id is null
;
alter table Usersettings.RegionalData
drop foreign key regionaldata_ibfk_2;

alter table Usersettings.RegionalData
add constraint FK_RegionalData_FirmCode foreign key (FirmCode) references Future.Suppliers(Id) on delete cascade;
