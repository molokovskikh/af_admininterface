alter table usersettings.RetClientsSet add column BuyingMatrixAction INTEGER default 0 not null;
alter table usersettings.RetClientsSet add column OfferMatrixAction INTEGER default 0 not null;
alter table usersettings.RetClientsSet add column BuyingMatrix INTEGER UNSIGNED;
alter table usersettings.RetClientsSet add column OfferMatrix INTEGER UNSIGNED;
