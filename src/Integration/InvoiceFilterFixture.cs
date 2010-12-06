using System.Linq;
using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Functional.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class InvoiceFilterFixture
	{
		[Test]
		public void Find_invoice_by_home_region()
		{
			Invoice invoice;
			List<Invoice> invoices;
			using (new SessionScope())
			{
				var client = DataMother.CreateTestClientWithAddressAndUser();
				client.Payer.JuridicalOrganizations[0].Recipient = Recipient.Queryable.First();
				client.Payer.JuridicalOrganizations[0].Save();
				invoice = new Invoice(client.Payer, Period.December);
				invoice.Save();
				var filter = new InvoiceFilter();
				filter.Region = client.HomeRegion;
				invoices = filter.Find();
			}

			Assert.That(invoices.Count, Is.EqualTo(1));
			Assert.That(invoices[0].Payer, Is.EqualTo(invoice.Payer));
		}
	}
}