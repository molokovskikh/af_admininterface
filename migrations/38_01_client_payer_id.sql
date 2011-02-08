alter table Future.Clients
drop foreign key FK_Clients_PayerId;
alter table Future.Clients
change column PayerId PayerId int unsigned;
