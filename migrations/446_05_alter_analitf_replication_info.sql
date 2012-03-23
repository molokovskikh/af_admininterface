alter table Usersettings.AnalitFReplicationInfo
drop foreign key FK_AnalitFReplicationInfo_2,
add constraint FK_AnalitFReplicationInfo_FirmCode foreign key (FirmCode) references Future.Suppliers(Id) on delete cascade;
