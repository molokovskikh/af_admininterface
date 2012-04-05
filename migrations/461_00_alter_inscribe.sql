alter table Usersettings.Inscribe
change column ClientCode AddressId int unsigned not null,
add column FECode int unsigned;
