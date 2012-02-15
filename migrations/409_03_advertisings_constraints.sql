alter table Billing.Advertisings
add constraint FK_Advertisings_Payer foreign key (Payer) references Billing.Payers(PayerId) on delete cascade;
