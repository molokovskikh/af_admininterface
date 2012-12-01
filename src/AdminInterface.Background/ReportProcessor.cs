using System;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Background
{
	public class ReportProcessor
	{
		public void Process()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback)) {
				ArHelper.WithSession(s => {
					s.CreateSQLQuery(
						@"delete a from Billing.Accounts a
left join Reports.general_reports gr on a.ObjectId = gr.GeneralReportCode
where gr.GeneralReportCode is null and a.Type = 2")
						.ExecuteUpdate();
				});
				scope.VoteCommit();
			}

			using (var scope = new TransactionScope(OnDispose.Rollback)) {
				ArHelper.WithSession(s => {
					var ids = s.CreateSQLQuery(
						@"select GeneralReportCode
from Reports.general_reports gr
left join Billing.Accounts a on a.ObjectId = gr.GeneralReportCode and a.Type = 2
where a.Id is null")
						.List<object>();
					foreach (var id in ids) {
						var report = s.Load<Report>(Convert.ToUInt32(id));
						var account = new ReportAccount(report);
						s.Save(account);
					}
				});
				scope.VoteCommit();
			}
		}
	}
}