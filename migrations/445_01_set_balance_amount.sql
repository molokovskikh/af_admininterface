update Billing.Payments
set BalanceAmount = Sum;

update Billing.Invoices
set BalanceAmount = PaidSum;

update Billing.BalanceOperations
set BalanceAmount = if(type = 0, -sum, sum);
