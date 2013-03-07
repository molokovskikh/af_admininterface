ALTER TABLE `usersettings`.`userpermissions` ADD COLUMN `OrderIndex` INT(3) UNSIGNED NOT NULL AFTER `SecurityMask`;

update `usersettings`.`userpermissions` set
OrderIndex = 1
where id = 27;

update `usersettings`.`userpermissions` set
OrderIndex = 2
where id = 29;

update `usersettings`.`userpermissions` set
OrderIndex = 3
where id = 31;

update `usersettings`.`userpermissions` set
OrderIndex = 4
where id = 33;

update `usersettings`.`userpermissions` set
OrderIndex = 5
where id = 37;

update `usersettings`.`userpermissions` set
OrderIndex = 6
where id = 51;

update `usersettings`.`userpermissions` set
OrderIndex = 7
where id = 39;

update `usersettings`.`userpermissions` set
OrderIndex = 8
where id = 41;

update `usersettings`.`userpermissions` set
OrderIndex = 9
where id = 53;

update `usersettings`.`userpermissions` set
OrderIndex = 10
where id = 43;

update `usersettings`.`userpermissions` set
OrderIndex = 11
where id = 45;

update `usersettings`.`userpermissions` set
OrderIndex = 12
where id = 47;

update `usersettings`.`userpermissions` set
OrderIndex = 13
where id = 49;

delete from `usersettings`.`userpermissions`
where id = 52;