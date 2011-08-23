update Billing.Invoices
set Customer = PayerName;

update Billing.Acts
set Customer = PayerName;
