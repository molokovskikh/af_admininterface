using System;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Background
{
	public class ReportProcessor
	{
		public void Process()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession(s => {
					var accounts = s.QueryOver<ReportAccount>()
						.Left.JoinQueryOver(a => a.Report)
						.WhereRestrictionOn(r => r.Id).IsNull
						.List<ReportAccount>();

					foreach (var account in accounts)
					{
						/* тк отчет удален мы не знаем какой у него был плательщик
						s.CreateSQLQuery("delete from Billing.PayerAuditRecords where ObjectId = :id and ObjectType = :type")
							.SetParameter("id", account.Payer.Id)
							.SetParameter("type", account.ObjectType)
							.ExecuteUpdate();
						*/

						account.Delete();
					}
				});
				scope.VoteCommit();
			}

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				ArHelper.WithSession(s => {
					var ids = s.CreateSQLQuery(
@"select GeneralReportCode
from Reports.general_reports gr
left join Billing.Accounts a on a.ObjectId = gr.GeneralReportCode and a.Type = 2
where a.Id is null")
						.List<object>();
					foreach (var id in ids)
					{
						var report = Report.Find(Convert.ToUInt32(id));
						var account = new ReportAccount(report);
						account.Save();
					}
				});
				scope.VoteCommit();
			}
		}
	}
}