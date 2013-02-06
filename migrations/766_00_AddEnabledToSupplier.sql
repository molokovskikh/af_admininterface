ALTER TABLE `customers`.`suppliers` MODIFY COLUMN `Id` INTEGER UNSIGNED NOT NULL DEFAULT 0,
 ADD COLUMN `Enabled` TINYINT(1) UNSIGNED NOT NULL DEFAULT 1 AFTER `VendorId`;

update `customers`.`suppliers` set enabled = if(disabled = 1, 0, 1);