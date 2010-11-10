using System.Linq;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class AddressFixture
	{
		[Test]
		public void Move_address()
		{
			var client = DataMother.CreateTestClientWithAddress();
			var recepient = DataMother.CreateTestClient();
			var address = client.Addresses.First();
			var legalEntity = recepient.Payer.JuridicalOrganizations.First();
			address.MoveToAnotherClient(recepient, legalEntity);

			ArHelper.WithSession(s => {
				s.CreateSQLQuery(@"update future.AddressIntersection ai
join future.Intersection i on ai.IntersectionId = i.Id
set ai.SupplierDeliveryId = '123'
where i.PriceId = 4832 and ai.AddressId = :addressId ")
					.SetParameter("addressId", address.Id)
					.ExecuteUpdate();
			});

			Assert.That(address.Client, Is.EqualTo(recepient));
			Assert.That(address.Payer, Is.EqualTo(recepient.Payer));
			Assert.That(address.LegalEntity, Is.EqualTo(legalEntity));
			ArHelper.WithSession(s => {
				var supplierDeliveryId = s.CreateSQLQuery(@"
select ai.SupplierDeliveryId
from future.AddressIntersection ai
join future.Intersection i on ai.IntersectionId = i.Id
where i.PriceId = 4832 and ai.AddressId = :addressId ")
					.SetParameter("addressId", address.Id)
					.UniqueResult<string>();
				Assert.That(supplierDeliveryId, Is.EqualTo("123"));
			});
		}
	}
}