using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	public class OffersFixture : IntegrationFixture
	{
		[Test]
		public void Search_should_return_offers()
		{
			var client = DataMother.TestClient();
			var offers = Offer.Search(client, "папа");
			Assert.That(offers.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Request_for_not_exist_name_should_return_zero_elements()
		{
			var client = DataMother.TestClient();
			var offers = Offer.Search(client, "sdfaefawefsdf");
			Assert.That(offers.Count, Is.EqualTo(0));
		}
	}
}
