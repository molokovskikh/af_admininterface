update logs.ClientsInfo i
join AccessRight.RegionalAdmins a on a.UserName = i.UserName
set i.Administrator = a.RowId
