insert into Billing.PayerClients(PayerId, ClientId)
select u.PayerId, u.ClientId
from Future.Users u
union
select a.PayerId, a.ClientId
from Future.Addresses a
group by PayerId, ClientId
