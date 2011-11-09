insert into Billing.Accounts(Type, Operator, Payment, BeAccounted, ReadyForAccounting, ObjectId)
select 3, 'kvasov', if(Segment = 1, 600, 0), if(Segment = 1, 0, 1), 1, Id from Future.Suppliers;
update Future.Suppliers s
join Billing.Accounts a on a.ObjectId = s.Id and a.Type = 3
set s.Account = a.Id;
