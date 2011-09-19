using System;
using System.Collections.Generic;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Tools.Calendar;
using Integration.Models;
using log4net;

namespace AdminInterface.Background
{
	public class Waiter : RepeatableCommand
	{
		private static ILog log = LogManager.GetLogger(typeof(Waiter));
		private List<Job> jobs = new List<Job>();

		public Waiter()
		{
			Delay = (int)TimeSpan.FromHours(1).TotalMilliseconds;
			Action = () => {
				new InvoiceProcessor().Process();

/*				using(new SessionScope())
					jobs.Each(j => j.Run());*/
			};
		}

		public void DoStart()
		{
			StandaloneInitializer.Init(typeof(InvoiceProcessor).Assembly);

			using(new SessionScope())
			{
				var job = new ActiveRecordJob("SendPaymentNotification", () => new SendPaymentNotification().Process());
				job.Plan(PlanPeriod.Month, 8.Day());
				jobs.Add(job);
				job = new ActiveRecordJob("SendPaymentNotification", () => new SendPaymentNotification().Process());
				job.Plan(PlanPeriod.Month, 15.Days());
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