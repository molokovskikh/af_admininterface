update Future.Addresses ad
join Billing.Accounting a on a.AccountId = ad.Id and a.Type = 1
set ad.AccountingId = a.Id;

update Future.Users u
join Billing.Accounting a on a.AccountId = u.Id and a.Type = 0
set u.AccountingId = a.Id;

update Billing.Accounting
set BeAccounted = 1, ReadyForAcounting = 1;
