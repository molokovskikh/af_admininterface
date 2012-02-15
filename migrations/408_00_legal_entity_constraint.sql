alter table Billing.LegalEntities
drop foreign key FK_LegalEntities_PayerId;

alter table Billing.LegalEntities
add constraint FK_LegalEntities_PayerId foreign key (PayerId) references Billing.Payers(PayerId) on delete cascade;
