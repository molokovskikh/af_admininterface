alter table Customers.Addresses
drop foreign key FK_Addresses_ContactGroupId;
alter table Customers.Addresses
add CONSTRAINT `FK_Addresses_ContactGroupId` FOREIGN KEY (`ContactGroupId`) REFERENCES `contacts`.`contact_groups` (`Id`);
