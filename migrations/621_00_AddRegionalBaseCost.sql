ALTER TABLE `usersettings`.`pricesregionaldata` ADD COLUMN `BaseCost` INTEGER UNSIGNED AFTER `Enabled`,
 ADD CONSTRAINT `FK_pricesregionaldata_BaseCost` FOREIGN KEY `FK_pricesregionaldata_BaseCost` (`BaseCost`)
    REFERENCES `pricescosts` (`CostCode`)
    ON DELETE SET NULL
    ON UPDATE RESTRICT;