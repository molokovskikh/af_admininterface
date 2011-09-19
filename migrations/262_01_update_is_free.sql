update Future.Users u
join Billing.Accounting a on a.Id = u.AccountingId
set a.IsFree = u.Free,
a.ObjectId = u.Id;

update Future.Addresses a
join Billing.Accounting c on c.Id = a.AccountingId
set c.IsFree = a.Free,
c.ObjectId = a.Id;
