alter table Billing.Recipients add column UserDescription VARCHAR(255) not null;
alter table Billing.Recipients add column AddressDescription VARCHAR(255) not null;
alter table Billing.Recipients add column ReportDescription VARCHAR(255) not null;
alter table Billing.Recipients add column SupplierDescription VARCHAR(255) not null;
