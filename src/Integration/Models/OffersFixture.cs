using AdminInterface.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	public class OffersFixture : Test.Support.IntegrationFixture
	{
		[Test, Ignore("В локальной базе нет предложений, нужно добавить код для создания предложений")]
		public void Search_should_return_offers()
		{
			var client = DataMother.CreateTestClientWithUser();
			var offers = Offer.Search(client.Users[0], "папа");
			Assert.That(offers.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Request_for_not_exist_name_should_return_zero_elements()
		{
			var client = DataMother.CreateTestClientWithUser();
			var offers = Offer.Search(client.Users[0], "sdfaefawefsdf");
			Assert.That(offers.Count, Is.EqualTo(0));
		}
	}
}