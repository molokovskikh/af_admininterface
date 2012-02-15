alter table Future.Intersection
drop foreign key `FK_Intersection_LegalEntityId`;
alter table Future.Intersection
add constraint `FK_Intersection_LegalEntityId` foreign key (LegalEntityId) references Billing.LegalEntities(Id) on delete cascade;
