alter table UserSettings.Defaults add column SmartOrderAssortmentPrice INTEGER UNSIGNED;

update Usersettings.Defaults
set SmartOrderAssortmentPrice = 4662;
