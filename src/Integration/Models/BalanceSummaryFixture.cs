using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class BalanceSummaryFixture : AdmIntegrationFixture
	{
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoices = payer.BuildInvoices(new DateTime(2011, 12, 10), new Period(2011, Interval.December))
				.Concat(payer.BuildInvoices(new DateTime(2012, 1, 10), new Period(2012, Interval.January)))
				.ToArray();

			invoices.SelectMany(i => i.Parts).Each(p => p.Process());

			Save(invoices);

			Flush();
		}

		[Test]
		public void Include_in_total_prev_year_result()
		{
			Assert.That(payer.Balance, Is.EqualTo(-1600));
			var summary = new BalanceSummary(new DateTime(2012, 1, 1), new DateTime(2012, 12, 31), payer);
			Assert.That(summary.Before, Is.EqualTo(-800));
			Assert.That(summary.Total, Is.EqualTo(-1600));
		}

		[Test]
		public void Calculate_total()
		{
			var summary = new BalanceSummary(new DateTime(2012, 1, 1), new DateTime(2012, 12, 31), payer);
			Assert.That(summary.TotalInvoice, Is.EqualTo(800));
		}
	}
}