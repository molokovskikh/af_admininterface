﻿using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Helpers;
using NHibernate.Linq;

namespace AdminInterface.Background
{
	public class ReportLogsTask : Task
	{
		protected override void Process()
		{
			var state = Session.Query<ReportLogProcessorState>().FirstOrDefault();
			if (state == null)
				state = new ReportLogProcessorState();

			var begin = state.LastRun;
			state.LastRun = DateTime.Now;

			var allowChanges = Session.CreateSQLQuery(@"
select gr.PayerId
from logs.GeneralReportLogs l
join reports.general_reports gr on gr.GeneralReportCode = l.GeneralReportCode
where l.LogTime >= :begin and gr.PayerId is not null and (l.Allow is not null or l.PayerId is null)")
				.SetParameter("begin", begin)
				.List<uint>();

			var payerChanges = Session.CreateSQLQuery(@"
select l.PayerId
from logs.GeneralReportLogs l
where l.LogTime >= :begin and l.PayerId is not null")
				.SetParameter("begin", begin)
				.List<uint>();

			var payerIds = allowChanges.Concat(payerChanges).Distinct().ToArray();

			foreach (var payerId in payerIds) {
				var payer = Session.Load<Payer>(payerId);
				payer.UpdatePaymentSum();
				Session.Save(payer);
			}
			Session.Save(state);
		}
	}
}