delete p from Usersettings.PricesData p
left join Future.Suppliers s on s.Id = p.FirmCode
where s.Id is null;

alter table Usersettings.PricesData
drop foreign key pricesdata_ibfk_1;

alter table Usersettings.PricesData
add constraint FK_PricesData_FirmCode foreign key (FirmCode) references Future.Suppliers(Id) on delete cascade;
