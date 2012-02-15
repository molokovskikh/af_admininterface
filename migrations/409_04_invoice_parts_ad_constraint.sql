alter table Billing.InvoiceParts
add constraint FK_InvoiceParts_Ad foreign key (Ad) references Billing.Advertisings(Id) on delete set null;
