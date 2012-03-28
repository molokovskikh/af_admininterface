update Billing.Invoices
set BalanceAmount = -BalanceAmount
where BalanceAmount > 0;
