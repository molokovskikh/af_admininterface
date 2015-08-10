alter table Customers.Users drop foreign key FK_Users_ContactGroupId;
alter table Customers.Users
add CONSTRAINT `FK_Users_ContactGroupId` FOREIGN KEY (`ContactGroupId`) REFERENCES `contacts`.`contact_groups` (`Id`);
