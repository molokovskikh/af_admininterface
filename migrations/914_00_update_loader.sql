update OrderSendRules.smart_order_rules
set Loader = 2
where ParseAlgorithm not in ('TextSource', 'ExcelSource', 'DbfSource')
