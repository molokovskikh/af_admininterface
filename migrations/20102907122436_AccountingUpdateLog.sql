
alter table Logs.AccountingLogs
add column Payment decimal(10,0),
add column BeAccounted tinyint(1),
add column ReadyForAcounting tinyint(1)
;
;
