using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Background
{
	public class ReportLogsProcessor
	{
		public void Process()
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				var state = ReportLogProcessorState.Queryable.FirstOrDefault();
				if (state == null)
					state = new ReportLogProcessorState();

				var begin = state.LastRun;
				state.LastRun = DateTime.Now;

				var payerIds = ArHelper.WithSession(s => {
					var allowChanges = s.CreateSQLQuery(@"
select gr.PayerId
from logs.GeneralReportLogs l
join reports.general_reports gr on gr.GeneralReportCode = l.GeneralReportCode
where l.LogTime >= :begin and gr.PayerId is not null and (l.Allow is not null or l.PayerId is null)")
						.SetParameter("begin", begin)
						.List<uint>();

					var payerChanges = s.CreateSQLQuery(@"
select l.PayerId
from logs.GeneralReportLogs l
where l.LogTime >= :begin and l.PayerId is not null")
						.SetParameter("begin", begin)
						.List<uint>();

					return allowChanges.Concat(payerChanges).Distinct().ToArray();
				});

				foreach (var payerId in payerIds)
				{
					var payer = Payer.Find(payerId);
					payer.UpdatePaymentSum();
					payer.Save();
				}
				state.Save();
				scope.VoteCommit();
			}
		}
	}
}