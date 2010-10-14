update future.users u
join future.clients c on c.id = u.clientid
set u.PayerId = c.PayerId
where u.payerid is null;

update future.addresses a
join billing.legalentities le on le.id = a.legalentityid
set a.PayerId = le.PayerId
where a.PayerId is null;
