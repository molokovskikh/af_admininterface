using System;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class InvoiceFilterFixture : AdmIntegrationFixture
	{
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
		}

		[Test]
		public void Find_invoice_by_home_region()
		{
			var invoice = new Invoice(payer, new Period(2010, Interval.December), DateTime.Now);
			session.Save(invoice);
			var filter = new PayerDocumentFilter { Region = payer.Clients.First().HomeRegion };
			var invoices = filter.Find<Invoice>(session);

			Assert.That(invoices.Count, Is.GreaterThan(0));
			Assert.That(invoices.First(i => i.Id == invoice.Id).Payer, Is.EqualTo(invoice.Payer));
		}

		[Test]
		public void Calculate_sum()
		{
			DataMother.TestClient(c => {
				c.Payers.Clear();
				c.Payers.Add(payer);
			});
			var act = new Act(payer, DateTime.Now);
			act.Parts.Add(new ActPart(act) {
				Name = "Тест",
				Count = 1,
				Cost = 100
			});
			act.CalculateSum();
			session.Save(act);

			var filter = new PayerDocumentFilter {
				Region = payer.Clients.First().HomeRegion,
				SearchText = payer.Id.ToString()
			};
			var acts = filter.Find<Act>(session);
			Assert.AreEqual(100, filter.Sum);
			Assert.AreEqual(1, acts.Count);
		}

		[Test]
		public void Try_find_by_payer_id()
		{
			var payer1 = payer;
			var payer2 = DataMother.CreatePayerForBillingDocumentTest();
			var invoice1 = new Invoice(payer1, new Period(2010, Interval.December), DateTime.Now);
			session.Save(invoice1);
			var invoice2 = new Invoice(payer2, new Period(2010, Interval.December), DateTime.Now);
			session.Save(invoice2);

			var filter = new PayerDocumentFilter {
				SearchText = new[] { payer1, payer2 }.Implode(p => p.Id)
			};
			var invoices = filter.Find<Invoice>(session);
			Assert.That(invoices, Is.EquivalentTo(new[] { invoice1, invoice2 }));
		}

		[Test]
		public void FindByCreatedOnTest()
		{
			var created = DateTime.Now;
			var invoice = new Invoice(payer, DateTime.Now);
			session.Save(invoice);
			var filter = new PayerDocumentFilter {
				CreatedOn = created
			};
			var result = filter.Find<Invoice>(session);
			Assert.That(result.Any(invoice1 => invoice1.Id == invoice.Id), Is.True);
		}
	}
}