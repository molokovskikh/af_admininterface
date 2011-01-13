alter table Billing.Payers
change column EmailInvoice EmailInvoice tinyint(1) not null default 0,
change column PrintInvoice PrintInvoice tinyint(1) not null default 0;
