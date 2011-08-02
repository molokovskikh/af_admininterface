update logs.clientsinfo l
join Future.Services s on s.Id = l.ClientCode
left join Future.Users u on u.Id = l.UserId
set l.ServiceId = ClientCode,
l.ObjectId = ifnull(UserId, ClientCode),
l.Name = if(u.Id is null, s.Name, ifnull(u.Name, u.Login)),
l.Type = if(u.Id is null, s.Type, 2)
