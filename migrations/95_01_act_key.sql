alter table Billing.Invoices
add constraint `FK_Invoices_Act` foreign key (Act) references Billing.Acts(Id) on delete set null;

alter table Billing.Advertisings
add constraint `FK_Advertisings_Act` foreign key (Act) references Billing.Acts(Id) on delete set null;

alter table Billing.Advertisings
add constraint `FK_Advertisings_Invoice` foreign key (Invoice) references Billing.Invoices(Id) on delete set null;
