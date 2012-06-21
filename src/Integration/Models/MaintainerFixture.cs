using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class MaintainerFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Create_intersection_after_legal_entity_creation()
		{
			var client = DataMother.TestClient();
			var org = new LegalEntity {
				Name = "тараканов и сыновья",
				Payer = client.Payers.First()
			};
			org.Save();
			Maintainer.LegalEntityCreated(org);
			var count = ArHelper.WithSession(s =>
				s.CreateSQLQuery(@"select count(*) from Customers.Intersection where LegalEntityId = :LegalEntityId")
					.SetParameter("LegalEntityId", org.Id)
					.UniqueResult<long>()
			);
			Assert.That(count, Is.GreaterThan(0));
		}

		[Test]
		public void Copy_cost_settings_from_root()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.Prices.First().AddCost();
			});
			supplier.Save();

			var client = DataMother.TestClient();

			var price = supplier.Prices.First();
			var intersection = Intersection.Queryable.Single(i => i.Client == client && i.Price == price);
			Assert.That(intersection.Cost, Is.EqualTo(price.Costs.First()));
			intersection.Cost = price.Costs[1];
			intersection.SaveAndFlush();

			var org = new LegalEntity("тараканов и сыновья", client.Payers.First());
			org.Save();
			Maintainer.LegalEntityCreated(org);

			intersection = Intersection.Queryable.Single(i => i.Client == client && i.Price == price && i.Org == org);
			Assert.That(intersection.Cost.Id, Is.EqualTo(price.Costs[1].Id), "идентификатор intersection {0}", intersection.Id);
		}
	}
}