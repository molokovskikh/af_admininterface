using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using Integration.Models;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class InvoiceFilterFixture : IntegrationFixture
	{
		[Test]
		public void Find_invoice_by_home_region()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var invoice = new Invoice(payer, Period.December, DateTime.Now);
			invoice.Save();
			var filter = new PayerDocumentFilter {Region = payer.Clients.First().HomeRegion};
			var invoices = filter.Find<Invoice>();

			Assert.That(invoices.Count, Is.GreaterThan(0));
			Assert.That(invoices.First(i => i.Id == invoice.Id).Payer, Is.EqualTo(invoice.Payer));
		}
	}
}