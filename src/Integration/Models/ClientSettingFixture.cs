using System;
using System.Linq;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ClientSettingFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Set_replication_if_matrix_settings_changed()
		{
			var supplier = DataMother.CreateSupplier();
			var price = supplier.Prices[0];
			Save(supplier);
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];

			Flush();

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();

			client.Settings.BuyingMatrixPrice = price;
			client.Settings.BuyingMatrixType = BuyingMatrixType.BlackList;
			client.Settings.WarningOnBuyingMatrix = BuyingMatrixAction.Block;
			client.Settings.SaveAndFlush();

			var info = session.CreateSQLQuery("select ForceReplication from Usersettings.AnalitfReplicationInfo where UserId = :UserId")
				.SetParameter("UserId", user.Id)
				.List<object>()
				.Select(v => Convert.ToBoolean(v))
				.ToList();
			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new [] {true}));
		}

		[Test(Description = "при изменении флага должно сбрасываться состояние поля ReclameDate")]
		public void ClearReclameDateAfterChange()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];

			Flush();

			session
				.CreateSQLQuery("update Usersettings.UserUpdateInfo set ReclameDate = now() where UserId = :UserId")
				.SetParameter("UserId", user.Id)
				.ExecuteUpdate();

			client.Settings.ShowAdvertising = !client.Settings.ShowAdvertising;
			client.Settings.SaveAndFlush();

			var reclameDate = session.CreateSQLQuery("select ReclameDate from Usersettings.UserUpdateInfo where UserId = :UserId")
				.SetParameter("UserId", user.Id)
				.UniqueResult();
			Assert.That(reclameDate, Is.Null, "Значение поля ReclameDate должно быть сброшено в null после изменения поля ShowAdvertising, чтобы пользователь получал рекламу заново");
		}

	}
}