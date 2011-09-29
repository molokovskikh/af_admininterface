﻿using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
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
			var client = new Client(new Payer());
			user.Init(client);
			Assert.That(user.AssignedPermissions, Is.Empty);
		}

		[Test]
		public void Init_with_permissions()
		{
			var user = new User();
			var client = new Client(new Payer());
			var permission = new UserPermission();
			user.AddPermission(permission);
			user.Init(client);
			Assert.That(user.AssignedPermissions.Count, Is.EqualTo(1));
		}
	}
}
