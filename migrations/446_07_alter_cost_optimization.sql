alter table Usersettings.CostOptimizationConcurrents
drop foreign key FK_CostOptimizationConcurrents_SupplierId;

alter table Usersettings.CostOptimizationConcurrents
add constraint FK_CostOptimizationConcurrents_SupplierId foreign key (SupplierId) references Future.Suppliers(Id) on delete cascade;

alter table Usersettings.CostOptimizationRules
drop foreign key FK_CostOptimizationRules_SupplierId;

alter table Usersettings.CostOptimizationRules
add constraint FK_CostOptimizationRules_SupplierId foreign key (SupplierId) references Future.Suppliers(Id) on delete cascade;

