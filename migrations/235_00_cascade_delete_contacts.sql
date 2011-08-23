ALTER TABLE `contacts`.`contact_groups`
 DROP FOREIGN KEY `FK_contact_groups_ContactGroupOwnerId`;

ALTER TABLE `contacts`.`contact_groups` ADD CONSTRAINT `FK_contact_groups_ContactGroupOwnerId` FOREIGN KEY `FK_contact_groups_ContactGroupOwnerId` (`ContactGroupOwnerId`)
    REFERENCES `contact_group_owners` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

ALTER TABLE `contacts`.`contact_groups`
 DROP FOREIGN KEY `FK_contact_groups_ContactOwnerId`;

ALTER TABLE `contacts`.`contact_groups` ADD CONSTRAINT `FK_contact_groups_ContactOwnerId` FOREIGN KEY `FK_contact_groups_ContactOwnerId` (`Id`)
    REFERENCES `contact_owners` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

ALTER TABLE `contacts`.`contacts`
 DROP FOREIGN KEY `FK_contacts_ContactOwnerId`;

ALTER TABLE `contacts`.`contacts` ADD CONSTRAINT `FK_contacts_ContactOwnerId` FOREIGN KEY `FK_contacts_ContactOwnerId` (`ContactOwnerId`)
    REFERENCES `contact_owners` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

ALTER TABLE `contacts`.`persons`
 DROP FOREIGN KEY `FK_persons_ContactGroupId`;

ALTER TABLE `contacts`.`persons` ADD CONSTRAINT `FK_persons_ContactGroupId` FOREIGN KEY `FK_persons_ContactGroupId` (`ContactGroupId`)
    REFERENCES `contact_groups` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

ALTER TABLE `contacts`.`persons`
 DROP FOREIGN KEY `FK_persons_ContactOwnerId`;

ALTER TABLE `contacts`.`persons` ADD CONSTRAINT `FK_persons_ContactOwnerId` FOREIGN KEY `FK_persons_ContactOwnerId` (`Id`)
    REFERENCES `contact_owners` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

