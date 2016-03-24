alter table Customers.Suppliers add column IsFederal TINYINT(1) default 0 not null;
alter table Customers.Suppliers add index (ContactGroupOwnerId), add constraint FK_Customers_Suppliers_ContactGroupOwnerId foreign key (ContactGroupOwnerId) references contacts.contact_group_owners (Id);
