using System;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class InvoiceFilterFixture : IntegrationFixture
	{
		[Test]
		public void Find_invoice_by_home_region()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, Period.December, DateTime.Now);
			invoice.Save();
			var filter = new PayerDocumentFilter {Region = payer.Clients.First().HomeRegion};
			var invoices = filter.Find<Invoice>();

			Assert.That(invoices.Count, Is.GreaterThan(0));
			Assert.That(invoices.First(i => i.Id == invoice.Id).Payer, Is.EqualTo(invoice.Payer));
		}

		[Test]
		public void Try_find_by_payer_id()
		{
			var payer1 = DataMother.CreatePayerForBillingDocumentTest();
			var payer2 = DataMother.CreatePayerForBillingDocumentTest();
			var invoice1 = new Invoice(payer1, Period.December, DateTime.Now);
			invoice1.Save();
			var invoice2 = new Invoice(payer2, Period.December, DateTime.Now);
			invoice2.Save();

			var filter = new PayerDocumentFilter {
				SearchText = new [] {payer1, payer2}.Implode(p => p.PayerID)
			};
			var invoices = filter.Find<Invoice>();
			Assert.That(invoices, Is.EquivalentTo(new [] {invoice1, invoice2}));
		}
	}
}