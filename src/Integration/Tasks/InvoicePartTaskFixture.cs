using System;
using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class InvoicePartTaskFixture : Test.Support.IntegrationFixture
	{
		private InvoicePartTask processor;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			processor = new InvoicePartTask();
		}

		[Test]
		public void Process_invoice_part_ready_for_processing()
		{
			var invoice = BuildInvoice(DateTime.Now.Date);

			processor.Execute();

			session.Refresh(payer);

			Assert.That(payer.Balance, Is.EqualTo(-800));
			session.Refresh(invoice);
			Assert.That(invoice.Parts[0].Processed, Is.True);
		}

		[Test]
		public void Do_not_process_part_not_ready_for_processing()
		{
			var invoice = BuildInvoice(DateTime.Now.AddDays(1));

			processor.Execute();

			Assert.That(payer.Balance, Is.EqualTo(0));
			session.Refresh(invoice);
			Assert.That(invoice.Parts[0].Processed, Is.False);
		}

		private Invoice BuildInvoice(DateTime date)
		{
			var invoices = payer.BuildInvoices(date, date.ToPeriod());

			Assert.That(invoices.Count(), Is.EqualTo(1));
			var invoice = invoices.Single();
			Assert.That(invoice.Sum, Is.EqualTo(800));

			session.Save(invoice);
			return invoice;
		}
	}
}