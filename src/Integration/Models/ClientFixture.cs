using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.log4net;
using PriceType = AdminInterface.Models.Suppliers.PriceType;

namespace Integration.Models
{
	[TestFixture]
	public class ClientFixture : IntegrationFixture
	{
		[Test]
		public void ResetUinTest()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();

			var info = user.UserUpdateInfo;
			info.AFCopyId = "123";
			session.Save(info);
			Flush();

			Assert.That(client.HaveUin(), Is.True);

			client.ResetUin();

			session.Refresh(user.UserUpdateInfo);
			Assert.That(user.UserUpdateInfo.AFCopyId, Is.Empty);
			Assert.That(client.HaveUin(), Is.False);
		}

		[Test]
		public void Update_firm_code_only()
		{
			var client = DataMother.CreateTestClientWithUser();
			var supplier = DataMother.CreateSupplier();
			session.Save(supplier);

			client.Settings.NoiseCosts = true;
			session.SaveOrUpdate(client.Settings);
			Assert.That(client.Settings.FirmCodeOnly, Is.EqualTo(0));

			client.Settings.NoiseCostExceptSupplier = supplier;
			session.SaveOrUpdate(client.Settings);
			Assert.That(client.Settings.FirmCodeOnly, Is.EqualTo(supplier.Id));

			client.Settings.NoiseCosts = false;
			session.SaveOrUpdate(client.Settings);
			Assert.That(client.Settings.FirmCodeOnly, Is.Null);
		}

		[Test]
		public void Change_client_payer()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var payer = DataMother.CreatePayer();
			session.SaveOrUpdate(payer);

			client.ChangePayer(session, payer, payer.JuridicalOrganizations.First());
			session.SaveOrUpdate(client);
			Assert.That(client.Payers, Is.EquivalentTo(new[] { payer }));
			Assert.That(client.Users[0].Payer, Is.EqualTo(payer));
			var address = client.Addresses[0];
			Assert.That(address.Payer, Is.EqualTo(payer));
			Assert.That(address.LegalEntity, Is.EqualTo(payer.JuridicalOrganizations[0]));
		}

		[Test]
		public void After_change_payer_update_payment_sum()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users.Each(u => u.Accounting.Accounted());
			var oldPayer = client.Payers.First();
			var newPayer = DataMother.CreatePayer();
			session.SaveOrUpdate(newPayer);

			client.ChangePayer(session, newPayer, newPayer.JuridicalOrganizations.First());
			session.SaveOrUpdate(client);

			Assert.That(oldPayer.PaymentSum, Is.EqualTo(0));
			Assert.That(newPayer.PaymentSum, Is.EqualTo(800));
		}

		[Test]
		public void Delete_client()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var clientId = client.Id;
			var payerId = client.Payers[0].Id;

			client.Delete(session);
			Reopen();

			Assert.That(session.Get<Client>(clientId), Is.Null);
			Assert.That(session.Get<Payer>(payerId), Is.Null);
		}

		[Test(Description = "Проверяет, что метод UpdatePricesForClient у клиента добавляет недостающие записи в UserPrices и включает должным образхом флаги в intersection")]
		public void UpdatePricesForClientTest()
		{
			var region = session.Query<Region>().First();
			var supplier = DataMother.CreateSupplier();
			session.Save(supplier);
			var client = DataMother.CreateTestClientWithAddressAndUser();
			session.Save(client);
			var intersection = session.Query<Intersection>().First(i => i.Client == client && i.Price.PriceType != PriceType.Vip);
			intersection.AgencyEnabled = false;
			intersection.AvailableForClient = false;
			session.Save(intersection);
			var vipPrice = supplier.AddPrice("vip", PriceType.Vip);
			session.Save(vipPrice);
			var vipIntersection = new Intersection { Client = client, Region = region, Price = vipPrice, Org = client.GetLegalEntity().First(), AgencyEnabled = false, AvailableForClient = false };
			session.Save(vipIntersection);

			session.CreateSQLQuery(@"delete from customers.userprices").ExecuteUpdate();

			client.UpdatePricesForClient(session);
			session.Refresh(intersection);
			Assert.IsTrue(intersection.AgencyEnabled);
			Assert.IsTrue(intersection.AvailableForClient);
			session.Refresh(vipIntersection);
			vipIntersection = session.Get<Intersection>(vipIntersection.Id);
			Assert.IsTrue(vipIntersection.AgencyEnabled);
			Assert.IsFalse(vipIntersection.AvailableForClient);

			var usePricesCount = session.CreateSQLQuery(@"
select count(*) from
customers.userprices u
where u.UserId = :userId
")
				.SetParameter("userId", client.Users[0].Id)
				.UniqueResult<long?>();
			var intCount = session.Query<Intersection>().Count(i => i.Client == client);
			Assert.AreEqual(usePricesCount, intCount);
		}
	}
}