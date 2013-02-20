ALTER TABLE `usersettings`.`defaults` ADD COLUMN `ProcessingAboutFirmBody` TEXT NOT NULL AFTER `TechOperatingModeEnd`,
 ADD COLUMN `ProcessingAboutNamesBody` TEXT NOT NULL AFTER `ProcessingAboutFirmBody`;

ALTER TABLE `usersettings`.`defaults` ADD COLUMN `ProcessingAboutFirmSubject` VARCHAR(255) NOT NULL AFTER `ProcessingAboutNamesBody`,
ADD COLUMN `ProcessingAboutNamesSubject` VARCHAR(255) NOT NULL AFTER `ProcessingAboutFirmSubject`;
