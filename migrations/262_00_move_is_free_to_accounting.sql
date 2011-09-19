alter table Billing.Accounting add column IsFree TINYINT(1) default 0 not null;
alter table Billing.Accounting add column ObjectId INTEGER UNSIGNED;
