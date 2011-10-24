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
			var user = new User();
			var client = new Client(new Payer(), new Region());
			user.Init(client);
			Assert.That(user.AssignedPermissions, Is.Empty);
		}

		[Test]
		public void Init_with_permissions()
		{
			var user = new User();
			var client = new Client(new Payer(), new Region());
			var permission = new UserPermission();
			user.AddPermission(permission);
			user.Init(client);
			Assert.That(user.AssignedPermissions.Count, Is.EqualTo(1));
		}
	}
}
