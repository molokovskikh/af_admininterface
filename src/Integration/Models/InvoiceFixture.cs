using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class InvoiceFixture : IntegrationFixture
	{
		[Test]
		public void Delete_invoice_for_ad()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var ad = new Advertising(payer, 1000);
			ad.SaveAndFlush();
			ad.Invoice = new Invoice(ad);
			ad.SaveAndFlush();
			ad.Invoice.Cancel();
			ad.Invoice.DeleteAndFlush();
		}
	}
}