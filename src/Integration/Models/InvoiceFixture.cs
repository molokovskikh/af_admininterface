using System;
using System.Linq;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class InvoiceFixture : AdmIntegrationFixture
	{
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
		}

		[Test]
		public void Delete_invoice_for_ad()
		{
			var ad = new Advertising(payer, 1000);
			session.Save(ad);
			ad.Invoice = new Invoice(ad);
			ad.Invoice.Parts[0].Processed = true;
			ad.Invoice.CalculateSum();
			session.Save(ad);
			session.Flush();

			Assert.That(payer.Balance, Is.EqualTo(-1000));
			ad.Invoice.DeleteAndFlush();
			Assert.That(payer.Balance, Is.EqualTo(0));
		}

		[Test]
		public void Do_not_save()
		{
			var ad = new Advertising(payer, 1000);
			payer.Ads.Add(ad);
			session.Save(ad);

			Reopen();
			payer = session.Load<Payer>(payer.Id);
			var invoices = payer.BuildInvoices(DateTime.Now, DateTime.Now.ToPeriod());
			var invoice = invoices.First();
			Close();
			Assert.That(invoice.Id, Is.EqualTo(0));
		}

		[Test]
		public void Include_report_into_invoice_with_custom_message()
		{
			var report = DataMother.Report(payer);
			report.Payment = 5000;
			report.Description = "Стат. отчет за {0}";
			session.Save(report);
			session.Refresh(payer);

			var invoiceDate = new DateTime(2011, 09, 11);
			var invoice = new Invoice(payer, invoiceDate.ToPeriod(), invoiceDate);
			Assert.That(invoice.Parts.Count, Is.EqualTo(2), invoice.Parts.Implode());
			var part = invoice.Parts[1];
			Assert.That(part.Sum, Is.EqualTo(5000));
			Assert.That(part.Name, Is.EqualTo("Стат. отчет за сентябрь"));
		}
	}
}