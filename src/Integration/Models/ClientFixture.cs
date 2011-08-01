using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;

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
			info.Update();
			scope.Flush();

			Assert.That(client.HaveUin(), Is.True);

			client.ResetUin();

			user.UserUpdateInfo.Refresh();
			Assert.That(user.UserUpdateInfo.AFCopyId, Is.Empty);
			Assert.That(client.HaveUin(), Is.False);
		}

		[Test]
		public void Update_firm_code_only()
		{
			var client = DataMother.CreateTestClientWithUser();
			client.Settings.NoiseCosts = true;
			client.Settings.Save();
			Assert.That(client.Settings.FirmCodeOnly, Is.EqualTo(0));

			var supplier = Supplier.Queryable.First();
			client.Settings.NoiseCostExceptSupplier = supplier;
			client.Settings.Save();
			Assert.That(client.Settings.FirmCodeOnly, Is.EqualTo(supplier.Id));

			client.Settings.NoiseCosts = false;
			client.Settings.Save();
			Assert.That(client.Settings.FirmCodeOnly, Is.Null);
		}

		[Test]
		public void Change_client_payer()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var payer = DataMother.CreatePayer();
			payer.Save();
			client.ChangePayer(payer, payer.JuridicalOrganizations.First());
			client.Save();
			Assert.That(client.Payers, Is.EquivalentTo(new []{payer}));
			Assert.That(client.Users[0].Payer, Is.EqualTo(payer));
			var address = client.Addresses[0];
			Assert.That(address.Payer, Is.EqualTo(payer));
			Assert.That(address.LegalEntity, Is.EqualTo(payer.JuridicalOrganizations[0]));
		}
	}
}