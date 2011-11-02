alter table logs.clientsinfo
drop key Administrator,
add constraint `FK_ClientsInfo_Administrator` foreign key (Administrator) references accessright.regionaladmins(RowId) on delete set null;
