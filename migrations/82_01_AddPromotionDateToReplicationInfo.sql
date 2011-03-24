alter table usersettings.AnalitFReplicationInfo
  add column `PromotionDate` datetime DEFAULT NULL;

alter table usersettings.PromotionCatalogs
  add column UpdateTime timestamp not null default current_timestamp on update current_timestamp;

update usersettings.PromotionCatalogs
set
  UpdateTime = current_timestamp();