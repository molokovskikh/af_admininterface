update usersettings.RetClientsSet
set NoiseCosts = 0
where NoiseCosts is null;
alter table usersettings.RetClientsSet
change column NoiseCosts NoiseCosts TINYINT(1) not null default 0;
