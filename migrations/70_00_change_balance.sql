alter table Billing.Payers
change column BeginBalance BeginBalance decimal not null default 0;
