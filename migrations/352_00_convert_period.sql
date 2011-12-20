alter table Billing.Invoices
change column Period Period char(8);

alter table Billing.Acts
change column Period Period char(8);
