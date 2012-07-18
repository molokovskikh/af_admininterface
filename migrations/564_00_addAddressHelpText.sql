alter table UserSettings.Defaults add column AddressesHelpText VARCHAR(255);

ALTER TABLE `usersettings`.`defaults` MODIFY COLUMN `AddressesHelpText` TEXT CHARACTER SET cp1251 COLLATE cp1251_general_ci DEFAULT NULL;

