use customers;
CREATE TABLE `rejectparsers` (
	`Id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(255) NULL DEFAULT NULL,
	`Supplier` INT(10) UNSIGNED NULL DEFAULT NULL,
	PRIMARY KEY (`Id`),
	INDEX `Supplier` (`Supplier`),
	CONSTRAINT `FK_Customers_RejectParsers_Supplier` FOREIGN KEY (`Supplier`) REFERENCES `suppliers` (`Id`) ON DELETE CASCADE
);


CREATE TABLE `rejectparserlines` (
	`Id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`Src` VARCHAR(255) NULL DEFAULT NULL,
	`Dst` VARCHAR(255) NULL DEFAULT NULL,
	`Parser` INT(10) UNSIGNED NULL DEFAULT NULL,
	PRIMARY KEY (`Id`),
	INDEX `Parser` (`Parser`),
	CONSTRAINT `FK_Customers_ParserLines_RejectParser` FOREIGN KEY (`Parser`) REFERENCES `rejectparsers` (`Id`) ON DELETE CASCADE
);

