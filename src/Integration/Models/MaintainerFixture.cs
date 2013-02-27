using System;
using System.Linq;
using AddUser;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NHibernate.Linq;
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
			Maintainer.LegalEntityCreated(org, session);
			var count = ArHelper.WithSession(s =>
				s.CreateSQLQuery(@"select count(*) from Customers.Intersection where LegalEntityId = :LegalEntityId")
					.SetParameter("LegalEntityId", org.Id)
					.UniqueResult<long>());
			Assert.That(count, Is.GreaterThan(0));
		}

		[Test]
		public void Copy_cost_settings_from_root()
		{
			var supplier = DataMother.CreateSupplier(s => { s.Prices.First().AddCost(); });
			Save(supplier);

			var client = DataMother.TestClient();

			var price = supplier.Prices.First();
			var baseCost = price.Costs.First();
			var notBaseCost = price.Costs[1];

			var intersection = session.Query<Intersection>().Single(i => i.Client == client && i.Price == price);
			Assert.That(intersection.Cost, Is.EqualTo(baseCost));
			intersection.Cost = notBaseCost;
			Save(intersection);
			var org = new LegalEntity("тараканов и сыновья", client.Payers.First());
			Save(org);
			Flush();

			Maintainer.LegalEntityCreated(org, session);

			intersection = session.Query<Intersection>().Single(i => i.Client == client && i.Price == price && i.Org == org);
			Assert.That(intersection.Cost.Id, Is.EqualTo(notBaseCost.Id), "идентификатор intersection {0}", intersection.Id);
		}

		[Test]
		public void Copy_supplier_code_from_base_price()
		{
			var supplier = DataMother.CreateSupplier();
			var price = supplier.AddPrice("Тестовый 1", PriceType.Regular);
			Save(supplier);

			var client = DataMother.CreateTestClientWithAddress();

			var intersection = session.Query<AddressIntersection>()
				.First(a => a.Intersection.Price == supplier.Prices[0] && a.Intersection.Client == client);
			intersection.SupplierDeliveryId = "d1";
			Save(intersection);

			intersection = session.Query<AddressIntersection>()
				.First(a => a.Intersection.Price == price && a.Intersection.Client == client);
			intersection.SupplierDeliveryId = "d2";
			Save(intersection);
			Flush();

			var data = managep.GetData(supplier);
			var table = data.Tables["Prices"];
			var row = table.NewRow();
			row["BuyingMatrix"] = 0;
			row["AgencyEnabled"] = 1;
			row["Enabled"] = 1;
			row["UpCost"] = 0;
			row["PriceType"] = 0;
			row["IsLocal"] = 0;
			table.Rows.Add(row);
			var message = "";
			(new managep()).Save(supplier, data, "", ref message);

			session.Refresh(supplier);
			Assert.That(supplier.Prices.Count, Is.EqualTo(3));
			intersection = session.Query<AddressIntersection>()
				.First(a => a.Intersection.Price == price && a.Intersection.Client == client);
			Assert.That(intersection.SupplierDeliveryId, Is.EqualTo("d2"));

			intersection = session.Query<AddressIntersection>()
				.First(a => a.Intersection.Price == supplier.Prices[2] && a.Intersection.Client == client);
			Assert.That(intersection.SupplierDeliveryId, Is.EqualTo("d1"));
		}
	}
}