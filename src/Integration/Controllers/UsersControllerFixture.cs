using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework.Test;
using Castle.MonoRail.TestSupport;
using Functional.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class UsersControllerFixture : BaseControllerTest
	{
		private UsersController controller;
		private User user1, user2;
		private Client client;
		private UserUpdateInfo info1, info2;

		[SetUp]
		public void Setup()
		{
			var admin = new Administrator
			{
				UserName = "TestAdmin",
				RegionMask = 1,
				AllowedPermissions = new List<Permission> {
					new Permission {Type = PermissionType.ChangePassword},
					new Permission {Type = PermissionType.ViewSuppliers},
					new Permission {Type = PermissionType.ViewDrugstore},
				},
			};
			SecurityContext.GetAdministrator = () => admin;
			controller = new UsersController();
			PrepareController(controller, "DoPasswordChange");
		}

		[Test]
		public void Change_user_password()
		{
			client = DataMother.CreateClientAndUsers();
			user1 = client.Users[0];
			user2 = client.Users[1];
			info1 = UserUpdateInfo.Find(user1.Id);
			info1.AFCopyId = "qwerty";
			info1.Update();
			info2 = UserUpdateInfo.Find(user2.Id);
			info2.AFCopyId = "12345";
			info2.Update();
			using (new SessionScope())
			{
				controller.DoPasswordChange(user1.Id, "", false, true, false, "");
			}
			info1.Refresh();
			info2.Refresh();
			Assert.That(info1.AFCopyId, Is.Empty);
			Assert.That(info2.AFCopyId, Is.EqualTo("12345"));
		}
	}
}
