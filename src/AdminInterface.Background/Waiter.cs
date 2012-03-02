using System;
using System.Collections.Generic;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.Models.Jobs;
using log4net;

namespace AdminInterface.Background
{
	public class Waiter : RepeatableCommand
	{
		private static ILog log = LogManager.GetLogger(typeof(Waiter));
		private List<ActiveRecordJob> jobs = new List<ActiveRecordJob>();

		public Waiter()
		{
			Delay = (int)TimeSpan.FromHours(1).TotalMilliseconds;
			Action = () => {
				new InvoiceProcessor().Process();
				new ReportProcessor().Process();
				new UpdateAccountProcessor().Process();
				new ReportLogsProcessor().Process();
				new InvoicePartProcessor().Process();

				using(new SessionScope())
					jobs.Each(j => j.Run());
			};
		}

		public void DoStart()
		{
			StandaloneInitializer.Init(typeof(InvoiceProcessor).Assembly);

			using(new SessionScope())
			{
				var job = new ActiveRecordJob(new SendPaymentNotification());
				jobs.Add(job);
			}

			Start();
		}

		public override void Error(Exception e)
		{
			log.Error(e);
		}
	}
}