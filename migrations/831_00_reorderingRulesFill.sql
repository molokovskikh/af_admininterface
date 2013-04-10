DROP TEMPORARY TABLE IF EXISTS usersettings.nulledReorders;

CREATE TEMPORARY TABLE usersettings.nulledReorders (
regionalDataId INT unsigned ) engine=MEMORY ;

insert into usersettings.nulledReorders
select rd.RowId from usersettings.RegionalData rd
left join usersettings.ReorderingRules rr on rr.RegionalDataId = rd.RowId
where rr.id is null
;

select regionalDataId from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Saturday', 504000000000 from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Friday', 684000000000 from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Thursday', 684000000000 from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Wednesday', 684000000000 from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Tuesday', 684000000000 from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Monday', 684000000000 from usersettings.nulledReorders;

insert into usersettings.ReorderingRules (RegionalDataId, DayOfWeek, TimeOfStopsOrders)
select regionalDataId, 'Sunday', 863400000000 from usersettings.nulledReorders;