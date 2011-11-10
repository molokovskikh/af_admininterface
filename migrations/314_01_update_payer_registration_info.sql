create temporary table payer_registration_info engine=memory
select PayerId, LogTime, OperatorName from Logs.PayerLogs l where Operation = 0;

update Billing.Payers p
join payer_registration_info r on r.payerid = p.payerid
set RegistrationDate = r.LogTime,
Registrant = r.OperatorName;
