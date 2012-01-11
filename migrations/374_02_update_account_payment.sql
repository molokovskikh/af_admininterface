update Billing.Accounts
set Payment = 0
where Payment is null;
