alter table Future.Addresses add column AccountingId INTEGER UNSIGNED;
alter table Billing.Accounting add column Payment NUMERIC(19,5);
alter table Billing.Accounting add column BeAccounted TINYINT(1) default 0 not null;
alter table Billing.Accounting add column ReadyForAcounting TINYINT(1) default 0 not null;
alter table future.Users add column AccountingId INTEGER UNSIGNED;
alter table Billing.Accounting change column `Operator` `Operator` varchar(255);