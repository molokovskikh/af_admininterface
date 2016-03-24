alter table farm.Regions add column Parent BIGINT UNSIGNED;
alter table farm.Regions add index (Parent), add constraint FK_farm_Regions_Parent foreign key (Parent) references farm.Regions (RegionCode);
