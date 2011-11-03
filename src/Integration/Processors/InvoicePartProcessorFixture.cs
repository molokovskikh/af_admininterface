using System;
using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Processors
{
	[TestFixture]
	public class InvoicePartProcessorFixture : IntegrationFixture
	{
		InvoicePartProcessor processor;
		Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			processor = new InvoicePartProcessor();
		}

		[Test]
		public void Process_invoice_part_ready_for_processing()
		{
			var invoice = BuildInvoice(DateTime.Now.Date);

			processor.Process();

			payer.Refresh();

			Assert.That(payer.Balance, Is.EqualTo(-800));
			invoice.Refresh();
			Assert.That(invoice.Parts[0].Processed, Is.True);
		}

		[Test]
		public void Do_not_process_part_not_ready_for_processing()
		{
			var invoice = BuildInvoice(DateTime.Now.AddDays(1));

			processor.Process();

			Assert.That(payer.Balance, Is.EqualTo(0));
			invoice.Refresh();
			Assert.That(invoice.Parts[0].Processed, Is.False);
		}

		private Invoice BuildInvoice(DateTime date)
		{
			var invoices = payer.BuildInvoices(date, Invoice.GetPeriod(date));

			Assert.That(invoices.Count(), Is.EqualTo(1));
			var invoice = invoices.Single();
			Assert.That(invoice.Sum, Is.EqualTo(800));

			invoice.Save();

			return invoice;
		}
	}
}