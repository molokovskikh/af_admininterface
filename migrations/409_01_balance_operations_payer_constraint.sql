alter table Billing.BalanceOperations
add constraint FK_BalanceOperations_Payer foreign key (Payer) references Billing.Payers(PayerId) on delete cascade;
