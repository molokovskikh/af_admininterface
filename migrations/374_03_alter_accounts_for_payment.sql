alter table Billing.Accounts
change column Payment Payment decimal(19, 5) not null default 0;
