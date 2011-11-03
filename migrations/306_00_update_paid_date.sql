update billing.Invoices I
join billing.invoiceparts ip on ip.invoice = i.id
set ip.Processed = 0,
ip.PayDate = concat('2011-11-', day(i.date))
where period = 3
and ip.name in ('Мониторинг оптового фармрынка за ноябрь', 'Обеспечение доступа к ИС (мониторингу фармрынка) в ноябре');


update billing.Invoices I
join billing.invoiceparts ip on ip.invoice = i.id
set ip.Processed = 0,
ip.PayDate = concat('2011-12-', day(i.date))
where period = 3
and ip.name in ('Мониторинг оптового фармрынка за декабрь', 'Обеспечение доступа к ИС (мониторингу фармрынка) в декабре');


update Billing.Payers p
set p.Balance = p.Balance +
(select sum(ip.Count * ip.Cost) from Billing.Invoices i
  join Billing.InvoiceParts ip on ip.Invoice = i.Id
  where i.Payer = p.PayerId and ip.Processed = 0)
;

update Billing.Invoices i
set PaidSum = ifnull((select sum(Count * Cost) from Billing.InvoiceParts ip where ip.Invoice = i.Id and ip.Processed = 1), 0);
