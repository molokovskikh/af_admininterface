insert into accessright.permissions (Name, `Type`, Shortcut)
values('Модерирование минипочты', 24, 'MMM');

set @insertId =  last_insert_id();

insert into accessright.adminspermissions (adminid, permissionid)
select rowid, @insertId from accessright.RegionalAdmins;