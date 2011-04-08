update Billing.Invoices i
join Billing.Acts a on a.Payer = i.Payer and a.Period = i.Period
set i.Act = a.Id
