alter table Future.Services add column Type INTEGER default 0 not null;
alter table Future.Services add column Name VARCHAR(255);
alter table Future.Services add column Disabled TINYINT(1) default 0 not null;
alter table Future.Services add column HomeRegion BIGINT UNSIGNED;
