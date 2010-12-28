using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class MaintainerFixture
	{
		[Test]
		public void Create_intersection_after_legal_entity_creation()
		{
			var client = DataMother.CreateTestClient();
			var legalEntity = new LegalEntity {
				Name = "тараканов и сыновья",
				Payer = client.Payer
			};
			legalEntity.Save();
			Maintainer.LegalEntityCreated(legalEntity);
			var count = ArHelper.WithSession(s =>
				s.CreateSQLQuery(@"select count(*) from future.Intersection where LegalEntityId = :LegalEntityId")
					.SetParameter("LegalEntityId", legalEntity.Id)
					.UniqueResult<long>()
			);
			Assert.That(count, Is.GreaterThan(0));
		}
	}
}