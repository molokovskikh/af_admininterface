using System;
using AdminInterface.Models.Billing;

namespace AdminInterface.Background
{
	public class ReportTask : Task
	{
		protected override void Process()
		{
			Session.CreateSQLQuery(@"delete a from Billing.Accounts a
left join Reports.general_reports gr on a.ObjectId = gr.GeneralReportCode
where gr.GeneralReportCode is null and a.Type = 2")
				.ExecuteUpdate();

			var ids = Session.CreateSQLQuery(
				@"select GeneralReportCode
from Reports.general_reports gr
left join Billing.Accounts a on a.ObjectId = gr.GeneralReportCode and a.Type = 2
where a.Id is null")
				.List<object>();
			foreach (var id in ids) {
				var report = Session.Load<Report>(Convert.ToUInt32(id));
				var account = new ReportAccount(report);
				Session.Save(account);
			}
		}
	}
}