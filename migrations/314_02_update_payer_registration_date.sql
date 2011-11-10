create temporary table creation_date engine=memory
SELECT p.PayerId, s.Registrant, s.RegistrationDate FROM (billing.Payers P, Future.Suppliers s)
where s.Id = (select min(ss.Id) from future.Suppliers ss where ss.Payer = p.PayerId);

create temporary table creation_date_copy engine=memory
select * from creation_date;

insert into creation_date
SELECT p.PayerId, c.Registrant, c.RegistrationDate
FROM (billing.Payers P, Future.Clients c)
left join creation_date_copy cd on cd.PayerId = p.PayerId
where c.Id = (select min(cc.Id) from future.Clients cc join Billing.PayerClients pc on pc.ClientId = cc.Id where pc.PayerId = p.PayerId)
and cd.PayerId is null;

update creation_date cd, billing.Payers P, Future.Clients c
set cd.RegistrationDate = c.RegistrationDate,
cd.Registrant = c.Registrant
where cd.PayerId = p.PayerId and c.Id = (select min(cc.Id) from future.Clients cc join Billing.PayerClients pc on pc.ClientId = cc.Id where pc.PayerId = p.PayerId)
and (cd.RegistrationDate is null or cd.RegistrationDate > c.RegistrationDate)
;

update Billing.Payers p
join creation_date cd on cd.PayerId = p.PayerId
set p.Registrant = cd.Registrant,
p.RegistrationDate = cd.RegistrationDate
where p.RegistrationDate is null;

update Billing.Payers p
set p.RegistrationDate = '2001-01-01'
where p.RegistrationDate is null;