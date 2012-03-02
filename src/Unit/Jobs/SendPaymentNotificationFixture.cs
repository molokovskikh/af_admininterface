using System;
using AdminInterface.Background;
using Common.Tools;
using NUnit.Framework;

namespace Unit.Jobs
{
	[TestFixture]
	public class SendPaymentNotificationFixture
	{
		private SendPaymentNotification job;

		[SetUp]
		public void Setup()
		{
			job = new SendPaymentNotification();
		}

		[Test]
		public void Plan_in_january_and_may_on_seven_days_late()
		{
			SystemTime.Now = () => new DateTime(2012, 1, 1);
			Assert.That(job.NextRun, Is.EqualTo(new DateTime(2012, 1, 15)));
			SystemTime.Now = () => new DateTime(2012, 1, 16);
			Assert.That(job.NextRun, Is.EqualTo(new DateTime(2012, 1, 22)));

			SystemTime.Now = () => new DateTime(2012, 5, 1);
			Assert.That(job.NextRun, Is.EqualTo(new DateTime(2012, 5, 15)));
		}

		[Test]
		public void Normal_plan()
		{
			SystemTime.Now = () => new DateTime(2012, 2, 1);
			Assert.That(job.NextRun, Is.EqualTo(new DateTime(2012, 2, 8)));
			SystemTime.Now = () => new DateTime(2012, 2, 15);
			Assert.That(job.NextRun, Is.EqualTo(new DateTime(2012, 2, 15)));
		}
	}
}