alter table billing.InvoiceParts add column PayDate DATETIME default 0 not null;
alter table billing.InvoiceParts add column Processed TINYINT(1) default 0 not null;
