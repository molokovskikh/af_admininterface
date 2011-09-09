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
		[Test]
		public void Delete_invoice_for_ad()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
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
			var payer = DataMother.CreatePayerForBillingDocumentTest();
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
	}
}