use logs;

ALTER TABLE `passwordchange`
ADD COLUMN `SmsLog` VARCHAR(255) NULL DEFAULT NULL AFTER `SentTo`;
