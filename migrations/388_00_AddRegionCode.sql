ALTER TABLE `future`.`services` ADD COLUMN `RegionCode` BIGINT(20) UNSIGNED AFTER `HomeRegion`;

update future.services s
set RegionCode = HomeRegion;