alter table Billing.InvoiceParts
add constraint `FK_Invoice_InvoiceParts` foreign key (Invoice) references Billing.Invoices(Id) on delete cascade;
