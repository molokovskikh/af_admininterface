using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using Common.Web.Ui.Models;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class PermissionFixture
	{
		[Test]
		public void Init()
		{
			var client = new Client(new Payer(), new Region());
			var user = new User(client);
			client.AddUser(user);
			Assert.That(user.AssignedPermissions, Is.Empty);
		}

		[Test]
		public void Init_with_permissions()
		{
			var client = new Client(new Payer(), new Region());
			var user = new User(client);
			client.AddUser(user);
			var permission = new UserPermission();
			user.AssignedPermissions.Add(permission);
			Assert.That(user.AssignedPermissions.Count, Is.EqualTo(1));
		}
	}
}
