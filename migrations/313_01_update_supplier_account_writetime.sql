update Billing.Accounts
set WriteTime = now()
where writetime is null and type = 3 and beaccounted = 1;
