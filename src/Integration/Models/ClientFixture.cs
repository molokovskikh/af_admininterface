using System.Linq;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ClientFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void ResetUinTest()
		{
			var client = CreateClient();
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

		private Client CreateClient()
		{
			var client = ForTest.CreateClient();
			client.BillingInstance.Save();
			client.Save();
			client.Users[0].Save();

			var updateInfo = new UserUpdateInfo(client.Users.First().Id);

			using (new TransactionScope())
				updateInfo.Create();
			return client;
		}
	}
}