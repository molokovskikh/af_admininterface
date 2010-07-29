using System.Linq;
using AdminInterface.Models;
using Functional.ForTesting;
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
	}
}