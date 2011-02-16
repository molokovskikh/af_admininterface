alter table logs.ClientsInfo
add column Administrator int,
add index (Administrator);
