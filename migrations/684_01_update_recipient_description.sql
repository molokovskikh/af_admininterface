update Billing.Recipients
set UserDescription = 'Мониторинг оптового фармрынка за {0}',
AddressDescription = 'Дополнительный адрес доставки медикаментов за {0}',
ReportDescription = 'Статистический отчет по фармрынку за {0}',
SupplierDescription = 'Справочно-информационные услуги за {0}'
where Id <> 4;

update Billing.Recipients
set UserDescription = 'Обеспечение доступа к ИС (мониторингу фармрынка) за {0}',
AddressDescription = 'Обеспечение доступа к ИС (мониторингу фармрынка) от доп.подразделений за {0}',
ReportDescription = 'Статистический отчет по фармрынку за {0}',
SupplierDescription = 'Справочно-информационные услуги за {0}'
where Id = 4;
