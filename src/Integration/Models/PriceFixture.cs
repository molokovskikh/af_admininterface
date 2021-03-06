﻿using System;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Integration.ForTesting;
using NHibernate.Linq;
using Test.Support;
using Test.Support.log4net;
using log4net.Config;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PriceFixture : AdmIntegrationFixture
	{
		[Test(Description = "при обновлении свойств Enabled AgencyEnabled должно устанавливаться свойство ForceReplication")]
		public void ChangePriceEnabledAndForceReplication()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			var price = supplier.Prices[0];

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();

			price.AgencyEnabled = false;
			price.Enabled = false;
			Save(price);

			Flush();

			CheckForceReplicationIsValue(supplier, true);

			ClearForceReplication(supplier);

			price.AgencyEnabled = true;
			Save(price);

			Flush();

			CheckForceReplicationIsValue(supplier, true);
		}

		[Test(Description = "при обновлении других свойств прайс-листа не должно устанавливаться свойство ForceReplication")]
		public void ChangeOtherPricePropertiesAndNotChangedForceReplication()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			var price = supplier.Prices[0];

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();

			price.Name = price.Name + " 123";
			Save(price);

			Flush();

			CheckForceReplicationIsValue(supplier, false);
		}

		[Test]
		public void Update_price_cost_type()
		{
			var supplier = DataMother.CreateSupplier();
			var price = supplier.Prices[0];
			price.CostType = 0;
			price.AddCost();
			Save(supplier);
			Assert.AreEqual(price.Costs[0].PriceItem, price.Costs[1].PriceItem);
			session.Flush();

			session.CreateSQLQuery("call UpdateCostType(:priceId, :costType)")
				.SetParameter("priceId", price.Id)
				.SetParameter("costType", 1)
				.ExecuteUpdate();

			session.Clear();
			price = session.Load<Price>(price.Id);
			Assert.AreEqual(price.CostType, 1);
			Assert.AreEqual(2, price.Costs.Count, price.Id.ToString());
			Assert.AreNotEqual(price.Costs[0].PriceItem, price.Costs[1].PriceItem);

			session.CreateSQLQuery("call UpdateCostType(:priceId, :costType)")
				.SetParameter("priceId", price.Id)
				.SetParameter("costType", 0)
				.ExecuteUpdate();

			session.Clear();
			price = session.Load<Price>(price.Id);
			Assert.AreEqual(price.CostType, 0);
			Assert.AreEqual(price.Costs[0].PriceItem, price.Costs[1].PriceItem);
		}


		[Test]
		public void Clean_matrix()
		{
			var supplier = DataMother.CreateSupplier();
			session.Save(supplier);
			var price = supplier.Prices[0];
			price.IsMatrix = true;
			price.PrepareSave();
			session.Save(price);
			session.Flush();

			var testPrice = session.Load<TestPrice>(price.Id);
			var product = new TestProduct("Тестовый продукт");
			session.Save(product);
			var item = new TestBuyingMatrix(testPrice, product);
			session.Save(item);
			Reopen();

			price = session.Load<Price>(price.Id);
			price.IsMatrix = false;
			price.PrepareSave();
			session.Save(price);
			session.Flush();
			Reopen();

			item = session.Get<TestBuyingMatrix>(item.Id);
			Assert.IsNull(item);
		}

		private void CheckForceReplicationIsValue(Supplier supplier, bool value)
		{
			var info = session.CreateSQLQuery("select ForceReplication from Usersettings.AnalitfReplicationInfo where FirmCode = :SupplierId")
				.SetParameter("SupplierId", supplier.Id)
				.List<object>()
				.Select(v => Convert.ToBoolean(v))
				.ToList();
			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new[] { value }));
		}

		private void ClearForceReplication(Supplier supplier)
		{
			session
				.CreateSQLQuery("update Usersettings.AnalitfReplicationInfo set ForceReplication = 0 where FirmCode = :SupplierId")
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();
		}
	}
}