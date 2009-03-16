using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class OffersFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void Search_should_return_offers()
		{
			var client = Client.Find(2575u);
			var offers = Offer.Search(client, "папа");
			Assert.That(offers.Count, Is.GreaterThan(0));
		}
	}
}
