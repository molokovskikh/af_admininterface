using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class BillingSearchItemFixture : IntegrationFixture
	{
		[Test]
		public void Search_payer()
		{
			var client = DataMother.TestClient();
			var payer = client.Payers.First();
			var recipient = Recipient.Queryable.First();
			payer.Recipient = recipient;
			payer.SaveAndFlush();

			var items = BillingSearchItem.FindBy(new BillingSearchProperties{
				SearchBy = SearchBy.BillingCode,
				SearchText = payer.Id.ToString(),
				RegionId = UInt64.MaxValue
			});
			Assert.That(items.Count, Is.EqualTo(1));
			var result = items[0];
			Assert.That(result.BillingCode, Is.EqualTo(payer.Id));
			Assert.That(result.Recipients, Is.EqualTo(recipient.Name));
		}

		[Test]
		public void Status_for_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			var payer = supplier.Payer;
			supplier.Save();

			var items = BillingSearchItem.FindBy(new BillingSearchProperties{
				SearchBy = SearchBy.BillingCode,
				SearchText = payer.Id.ToString(),
				RegionId = UInt64.MaxValue
			});
			Assert.That(items.Count, Is.EqualTo(1));
			var result = items[0];
			Assert.That(result.BillingCode, Is.EqualTo(payer.Id));
			Assert.That(result.IsDisabled, Is.False);
		}

		[Test]
		public void Search_by_recipient_id()
		{
			var items = BillingSearchItem.FindBy(new BillingSearchProperties{
				RegionId = UInt64.MaxValue,
				RecipientId = Recipient.Queryable.First().Id
			});
			Assert.That(items.Count, Is.GreaterThan(0));
		}
	}
}