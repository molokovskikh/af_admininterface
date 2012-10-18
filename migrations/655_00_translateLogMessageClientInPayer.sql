insert into billing.PayerAuditRecords
(UserName, WriteTime, ObjectId, ObjectType, Name, Message, Payer, `Comment`)
select cl.OperatorName, cl.LogTime, cl.ClientId, 1, c.Name,
concat("Изменено 'Включен' было ", if(cl.status, "'Отключен'", "'Включен'"), " стало ", if(cl.status, "'Включен'", "'Отключен'"))
, p.PayerId, cl.`Comment`
from logs.ClientLogs cl
join Customers.clients c on c.id = cl.ClientId
join billing.payerclients p on p.ClientId = c.id
where cl.status is not null
and cl.operation = 1;


insert into billing.PayerAuditRecords
(UserName, WriteTime, ObjectId, ObjectType, Name, Message, Payer, `Comment`)
select sl.OperatorName, sl.LogTime, sl.SupplierId, 0, s.Name,
concat("Изменено 'Включен' было ", if(sl.disabled, "'Отключен'", "'Включен'"), " стало ", if(sl.disabled, "'Включен'", "'Отключен'"))
, s.Payer, sl.`Comment`
from logs.SupplierLogs sl
join Customers.Suppliers s on s.id = sl.SupplierId
where sl.disabled is not null
and sl.operation = 1;