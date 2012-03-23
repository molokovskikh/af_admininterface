alter table Usersettings.SupplierPromotions
drop foreign key FK_SupplierPromotions_SupplierId;

alter table Usersettings.SupplierPromotions
add constraint FK_SupplierPromotions_SupplierId foreign key (SupplierId) references Future.Suppliers(Id) on delete cascade;
