alter table farm.Regions add column AddressPayment NUMERIC(19,5) default 0 not null;
alter table farm.Regions add column UserPayment NUMERIC(19,5) default 0 not null;
alter table farm.Regions add column SupplierUserPayment NUMERIC(19,5) default 0 not null;
