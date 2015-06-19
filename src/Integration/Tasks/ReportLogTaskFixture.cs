using System;
using AdminInterface.Background;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class ReportLogTaskFixture : AdmIntegrationFixture
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
			session.Save(payer);
			session.Save(account);

			payer.Reports.Add(report);
			account.Payment = 5000;
			session.Save(payer);
		}

		[Test]
		public void Recalculate_payment_sum_after_report_disable()
		{
			Assert.That(payer.PaymentSum, Is.EqualTo(5000));
			report.Allow = false;
			session.Save(report);
			Flush();
			Close();

			new ReportLogsTask().Execute();

			Reopen();
			payer = session.Load<Payer>(payer.Id);
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
		}

		[Test]
		public void Update_payment_sum_on_payer_change()
		{
			var newPayer = DataMother.CreatePayer();
			session.Save(newPayer);
			report.Payer = newPayer;
			session.Save(report);
			Flush();
			Close();

			new ReportLogsTask().Execute();

			Reopen();
			payer = session.Load<Payer>(payer.Id);
			Assert.That(payer.PaymentSum, Is.EqualTo(0));
			newPayer = session.Load<Payer>(newPayer.Id);
			Assert.That(newPayer.PaymentSum, Is.EqualTo(5000));
		}
	}
}