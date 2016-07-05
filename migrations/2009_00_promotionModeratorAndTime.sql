use usersettings;

	ALTER TABLE `supplierpromotions`
	ADD COLUMN `Moderator` VARCHAR(255) NULL DEFAULT NULL AFTER `Moderated`,
	ADD COLUMN `ModerationChanged` DATETIME NULL DEFAULT NULL AFTER `Moderator`;
	 	
	ALTER TABLE `defaults`
	ADD COLUMN `promotionModerationDeniedSubject` VARCHAR(255) NULL DEFAULT NULL AFTER `NewSupplierMailSubject`,
	ADD COLUMN `promotionModerationDeniedBody` TEXT NULL DEFAULT NULL AFTER `promotionModerationDeniedSubject`,
	ADD COLUMN `promotionModerationEscapeSubject` VARCHAR(255) NULL DEFAULT NULL AFTER `promotionModerationDeniedBody`,
	ADD COLUMN `promotionModerationEscapeBody` TEXT NULL DEFAULT NULL AFTER `promotionModerationEscapeSubject`,
	ADD COLUMN `promotionModerationAllowedSubject` VARCHAR(255) NULL DEFAULT NULL AFTER `promotionModerationEscapeBody`,
	ADD COLUMN `promotionModerationAllowedBody` TEXT NULL DEFAULT NULL AFTER `promotionModerationAllowedSubject`;