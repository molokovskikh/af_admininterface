alter table Usersettings.RetClientsSet
drop foreign key FK_RetClientsSet_NetworkSupplierId;

alter table Usersettings.RetClientsSet
add constraint FK_RetClientsSet_NetworkSupplierId foreign key (NetworkSupplierId) references Future.Suppliers(Id) on delete set null;
