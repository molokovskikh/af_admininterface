insert into accessright.permissions (id, name, `type`, shortcut)
values (28,"Отчеты для менеджеров", 23, "RFM");

insert into accessright.adminspermissions (adminid, permissionid)
values (85,28);