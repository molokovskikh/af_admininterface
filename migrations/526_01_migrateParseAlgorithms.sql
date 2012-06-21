insert into UserSettings.ParseAlgorithm (Name)
SELECT ParseAlgorithm FROM ordersendrules.smart_order_rules s
where ParseAlgorithm is not null
group by ParseAlgorithm;