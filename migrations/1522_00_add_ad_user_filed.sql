use customers;
ALTER TABLE `users`
	ADD COLUMN `FtpAccess` TINYINT NOT NULL DEFAULT '0' AFTER `Login`;
