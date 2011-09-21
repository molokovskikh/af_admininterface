using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Models
{
	[TestFixture]
	public class InvoiceFixture : IntegrationFixture
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
			ad.SaveAndFlush();
			ad.Invoice = new Invoice(ad);
			ad.SaveAndFlush();
			Assert.That(payer.Balance, Is.EqualTo(-1000));
			ad.Invoice.DeleteAndFlush();
			Assert.That(payer.Balance, Is.EqualTo(0));
		}

		[Test]
		public void Do_not_save()
		{
			var ad = new Advertising(payer, 1000);
			payer.Ads.Add(ad);
			ad.SaveAndFlush();

			Reopen();
			payer = Payer.Find(payer.Id);
			var invoices = payer.BuildInvoices(DateTime.Now, Invoice.GetPeriod(DateTime.Now));
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
			report.Save();

			var invoiceDate = new DateTime(2011, 09, 11);
			var invoice = new Invoice(payer, Invoice.GetPeriod(invoiceDate), invoiceDate);
			var part = invoice.Parts[1];
			Assert.That(part.Sum, Is.EqualTo(5000));
			Assert.That(part.Name, Is.EqualTo("Стат. отчет за сентябрь"));
		}
	}
}
