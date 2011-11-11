delete ip from Billing.InvoiceParts ip
left join Billing.Invoices i on i.Id = ip.Invoice
where i.Id is null;

