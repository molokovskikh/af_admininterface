alter table usersettings.SupplierPromotions
  add column Status tinyint(1) unsigned not null default '0';

update usersettings.SupplierPromotions
set
  Status = Enabled and not AgencyDisabled;
