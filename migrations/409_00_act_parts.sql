alter table Billing.ActParts
add constraint FK_ActsParts_Act foreign key (Act) references Billing.Acts(Id) on delete cascade;
