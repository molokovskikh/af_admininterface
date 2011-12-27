update Future.Users u
join Billing.Accounts a on a.Id = u.AccountingId
join Future.Services s on s.Id = u.RootService
set a.Payment = 0
where s.Type = 0;
