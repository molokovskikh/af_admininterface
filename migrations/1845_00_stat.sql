alter table Customers.Users add column DoNotCheckWellBeing TINYINT(1) default 0 not null;
alter table Customers.Users add column LastOrderSum NUMERIC(19,5) default 0 not null;
alter table Customers.Users add column OrderSumDelta NUMERIC(19,5) default 0 not null;
