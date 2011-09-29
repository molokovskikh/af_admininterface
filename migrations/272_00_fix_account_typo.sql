alter table Billing.Accounts
change column ReadyForAcounting ReadyForAccounting tinyint(1) not null default '0';
