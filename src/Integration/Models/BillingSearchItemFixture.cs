using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class BillingSearchItemFixture
	{
		[Test]
		public void Search_payer()
		{
			Payer payer;
			Recipient recipient;
			using (new SessionScope())
			{
				recipient = Recipient.Queryable.First();
				var client = DataMother.CreateTestClient();
				client.Payer.Recipient = recipient;
				client.Payer.Save();
				payer = client.Payer;
			}

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
	}
}