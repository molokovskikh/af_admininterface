update Billing.InvoiceParts
set Processed = 1;

update Billing.InvoiceParts p
join Billing.Invoices i on i.Id = p.Invoice
set p.PayDate = i.Date;
