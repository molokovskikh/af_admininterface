alter table Usersettings.RetClientsSet
  add column ShowCertificatesWithoutRefSupplier tinyint(1) unsigned not null default '0' comment 'Отображать сертификаты без привязки к поставщику';

update
  Usersettings.RetClientsSet,
  Future.Clients
set
  RetClientsSet.ShowCertificatesWithoutRefSupplier = Clients.ShowCertificatesWithoutRefSupplier
where
  RetClientsSet.ClientCode = Clients.Id;

alter table Future.Clients
  drop column ShowCertificatesWithoutRefSupplier;
