delete from Billing.Invoices
where PayerName is null;

alter table Billing.Invoices
change column PayerName PayerName varchar(255) not null;
