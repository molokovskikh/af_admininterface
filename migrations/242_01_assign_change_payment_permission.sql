use accessright;
insert into adminspermissions(AdminId, PermissionId)
select a.RowId, p.Id
from regionaladmins a, permissions p
where p.ShortCut = "BCP"
