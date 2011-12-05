update Billing.Accounts a
join Future.Users u on u.Id = a.ObjectId
join Future.Services s on s.Id = u.RootService
set BeAccounted = 1,
ReadyForAccounting = 1,
WriteTime = now(),
Operator = 'kvasov'
where a.Type = 0 and s.Type = 0;

