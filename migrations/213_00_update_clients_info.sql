alter table logs.clientsinfo add column ServiceId INTEGER UNSIGNED default 0 not null;
alter table logs.clientsinfo add column ObjectId INTEGER UNSIGNED default 0 not null;
alter table logs.clientsinfo add column Type INTEGER default 0 not null;
alter table logs.clientsinfo add column Name VARCHAR(255);
