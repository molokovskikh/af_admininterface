using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class PermissionFixture
	{
		[Test]
		public void Test()
		{
			var user = new User();
			var client = new Client();
			user.InitUser(client);
			Assert.That(user.AssignedPermissions, Is.Empty);
		}

		[Test]
		public void Test1()
		{
			var user = new User();
			var client = new Client();
			var permission = new UserPermission();
			user.AddPermission(permission);  
   			user.InitUser(client);
			Assert.That(user.AssignedPermissions.Count, Is.EqualTo(1));
		}
	}
}
