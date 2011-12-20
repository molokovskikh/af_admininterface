update Billing.Invoices
set Period = concat('2011-', Period);

update Billing.Acts
set Period = concat('2011-', Period);
