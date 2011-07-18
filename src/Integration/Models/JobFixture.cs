using System;
using Common.Tools;
using Common.Tools.Calendar;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ActiveRecordJobFixture : IntegrationFixture
	{
		[Test]
		public void Load_active_record_job()
		{
			var name = String.Format("TestJob{0}", DateTime.Now);
			var job = new ActiveRecordJob(name, () => {});
			job.Plan(PlanPeriod.Month, 1.Day());
			Assert.That(job.Run(), Is.True);
			job = new ActiveRecordJob(name, () => {});
			job.Plan(PlanPeriod.Month, 1.Day());
			Assert.That(job.Run(), Is.False);
		}
	}

	[TestFixture]
	public class JobFixture
	{
		private Job job;

		[SetUp]
		public void Setup()
		{
			job = new Job(() => {});
			SystemTime.Now = () => new DateTime(2011, 1, 1);
		}

		[Test]
		public void Plan_job()
		{
			job.Plan(PlanPeriod.Month, 10.Day());
			job.Run();
			SystemTime.Now = () => new DateTime(2011, 2, 1);
			Assert.That(job.Ready(), Is.False);
			SystemTime.Now = () => new DateTime(2011, 2, 10);
			Assert.That(job.Ready(), Is.True);
			job.Run();
			Assert.That(job.Ready(), Is.False);
		}

		[Test]
		public void Job_from_one_month()
		{
			job.Plan(PlanPeriod.Month, 10.Day());
			SystemTime.Now = () => new DateTime(2011, 2, 10);
			job.Run();
			SystemTime.Now = () => new DateTime(2011, 3, 10);
			Assert.That(job.Ready(), Is.True);
		}

		[Test]
		public void Replan_job()
		{
			job.Plan(PlanPeriod.Month, 1.Day());
			Assert.That(job.Run(), Is.True);
			job.Plan(PlanPeriod.Month, 10.Day());
			TimeMachine.ToFuture(9.Days());
			Assert.That(job.Run(), Is.True);
		}

		[Test]
		public void Reapet_run_at_same_day()
		{
			SystemTime.Now = () => new DateTime(2011, 7, 15, 15, 52, 48);
			job.Plan(PlanPeriod.Month, 15.Day());
			Assert.That(job.Run(), Is.True);
			SystemTime.Now = () => new DateTime(2011, 7, 15, 18, 09, 43);
			Assert.That(job.Run(), Is.False);
		}
	}
}