using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ClientFixture
	{
		[Test]
		public void ResetUinTest()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();

			var info = UserUpdateInfo.Find(user.Id);
			info.AFCopyId = "123";
			info.Update();

			Assert.That(client.HaveUin(), Is.True);

			client.ResetUin();

			var reloadedInfo = UserUpdateInfo.Find(user.Id);
			Assert.That(reloadedInfo.AFCopyId, Is.Empty);
			Assert.That(client.HaveUin(), Is.False);
		}

		[Test]
		public void Update_firm_code_only()
		{
			using (new SessionScope())
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
		}
	}
}