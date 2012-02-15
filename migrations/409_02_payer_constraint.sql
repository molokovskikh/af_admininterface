alter table Billing.Payments
add constraint FK_Payments_PayerId foreign key (PayerId) references Billing.Payers(PayerId) on delete cascade,
add constraint FK_Payments_Ad foreign key (Ad) references Billing.Advertisings(Id) on delete set null;
