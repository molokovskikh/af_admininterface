alter table Logs.document_logs
drop foreign key `FK_document_logs_FirmCode`;

alter table Logs.document_logs
add constraint `FK_document_logs_FirmCode` foreign key (FirmCode) references Future.Suppliers(Id) on delete cascade;
