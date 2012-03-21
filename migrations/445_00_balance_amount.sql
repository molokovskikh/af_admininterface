alter table Billing.Payments add column BalanceAmount NUMERIC(19,5) default 0 not null;
alter table Billing.BalanceOperations add column BalanceAmount NUMERIC(19,5) default 0 not null;
alter table billing.Invoices add column BalanceAmount NUMERIC(19,5) default 0 not null;
