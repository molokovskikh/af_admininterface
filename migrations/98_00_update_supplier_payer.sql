update future.suppliers s
join usersettings.clientsdata cd on cd.firmcode = s.id
set s.Payer = cd.BillingCode
