alter table customers.users
  add column `ExcludeFromManagerReports` TINYINT(1) UNSIGNED NOT NULL DEFAULT '0';