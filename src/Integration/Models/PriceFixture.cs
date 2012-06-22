using System;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Integration.ForTesting;
using Test.Support.log4net;
using log4net.Config;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class PriceFixture : Test.Support.IntegrationFixture
	{
		 
		[Test(Description = "при обновлении свойств Enabled AgencyEnabled должно устанавливаться свойство ForceReplication")]
		public void ChangePriceEnabledAndForceReplication()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var supplier = DataMother.CreateSupplier();
			supplier.Save();
			var price = supplier.Prices[0];

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();

			price.AgencyEnabled = false;
			price.Enabled = false;
			price.Save();

			Flush();

			CheckForceReplicationIsValue(supplier, true);

			ClearForceReplication(supplier);

			price.AgencyEnabled = true;
			price.Save();

			Flush();

			CheckForceReplicationIsValue(supplier, true);
		}

		[Test(Description = "при обновлении других свойств прайс-листа не должно устанавливаться свойство ForceReplication")]
		public void ChangeOtherPricePropertiesAndNotChangedForceReplication()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			var supplier = DataMother.CreateSupplier();
			supplier.Save();
			var price = supplier.Prices[0];

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();

			price.Name = price.Name + " 123";
			price.Save();

			Flush();

			CheckForceReplicationIsValue(supplier, false);
		}

		private void CheckForceReplicationIsValue(Supplier supplier, bool value)
		{
			var info = session.CreateSQLQuery("select ForceReplication from Usersettings.AnalitfReplicationInfo where FirmCode = :SupplierId")
				.SetParameter("SupplierId", supplier.Id)
				.List<object>()
				.Select(v => Convert.ToBoolean(v))
				.ToList();
			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new [] {value}));
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