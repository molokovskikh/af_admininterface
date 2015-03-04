use logs;
ALTER TABLE `requestlogs`
	DROP FOREIGN KEY `FK_logs_RequestLogs_UserId`;
ALTER TABLE `requestlogs`
	ADD CONSTRAINT `FK_logs_RequestLogs_UserId` FOREIGN KEY (`UserId`) REFERENCES `customers`.`users` (`Id`) ON DELETE CASCADE;