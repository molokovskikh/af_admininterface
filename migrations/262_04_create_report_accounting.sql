insert into Billing.Accounting(Payment, Type, ReadyForAcounting, ObjectId)
select 0, 2, 1, GeneralReportCode
from reports.general_reports;
