using AdminInterface.Background;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class ReportLogProcessorFixture : Test.Support.IntegrationFixture
	{
		private Payer payer;
		private Report report;
		private ReportAccount account;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayer();
			account = DataMother.Report(payer);
			report = account.Report;
			payer.Save();
			account.Save();

			payer.Reports.Add(report);
			account.Payment = 5000;
			payer.Save();
		}

		[Test]
		public void Recalculate_payment_sum_after_report_disable()
		{
			Assert.That(payer.PaymentSum, Is.EqualTo(5000));
			report.Allow = false;
			report.Save();
			Flush();
			Close();

			new ReportLogsProcessor().Process();

			Reopen();
			payer = Payer.Find(payer.Id);
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
		}

		[Test]
		public void Update_payment_sum_on_payer_change()
		{
			var newPayer = DataMother.CreatePayer();
			newPayer.Save();
			report.Payer = newPayer;
			report.Save();
			Flush();
			Close();

			new ReportLogsProcessor().Process();

			Reopen();
			payer = Payer.Find(payer.Id);
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
			newPayer = Payer.Find(newPayer.Id);
			Assert.That(newPayer.PaymentSum, Is.EqualTo(5000));
		}
	}
}