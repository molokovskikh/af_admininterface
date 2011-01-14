alter table Billing.Payments add column Comment VARCHAR(255);
alter table billing.Payers add column SendPaymentNotification TINYINT(1) default 0 not null;
