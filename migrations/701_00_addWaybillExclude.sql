
    create table usersettings.WaybillExcludeFile (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Mask VARCHAR(255),
       Supplier INTEGER UNSIGNED,
       primary key (Id)
    );
alter table usersettings.WaybillExcludeFile add index (Supplier), add constraint FK_usersettings_WaybillExcludeFile_Supplier foreign key (Supplier) references Customers.Suppliers (Id);
