using System;
using System.Collections.Generic;
using System.Configuration;
using AdminInterface.Mailers;
using AdminInterface.Models;
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
				var mailer = new MonorailMailer {
					SiteRoot = ConfigurationManager.AppSettings["SiteRoot"]
				};

				new SendInvoiceTask(mailer).Execute();
				new ReportTask().Execute();
				new UpdateAccountTask().Execute();
				new ReportLogsTask().Execute();
				new InvoicePartTask().Execute();

				using (new SessionScope())
					jobs.Each(j => j.Run());
			};
		}

		public void DoStart()
		{
			StandaloneInitializer.Init(typeof(SendInvoiceTask).Assembly);

			using (new SessionScope()) {
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