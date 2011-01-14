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
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
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

		[Test]
		public void Add_user()
		{
			client = DataMother.CreateTestClientWithUser();
			var client1 = DataMother.CreateClientAndUsers();
			var address = new Address
			{
				Client = client,
				Value = "тестовый адрес"
			};
			client.AddAddress(address);
			user1 = client.Users[0];
			var user = new User(client);
			var clientContacts = new[] {
				new Contact{Id = 1, Type = 0, ContactText = "4411@33.ru, hffty@jhg.ru"}};
			var regionSettings = new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}};
			var person = new[] {new Person()};
			using (new SessionScope())
			{
				controller.Add(user, clientContacts, regionSettings, address, person, "", true, client1.Id, "11@33.ru, hgf@jhgj.ut");	
			}
			var mails = ArHelper.WithSession(s => s
				.CreateSQLQuery(@"select sentto from logs.passwordchange where targetusername = :userId")
			    .SetParameter("userId", user.Id)
			    .UniqueResult());
 
			Assert.That(mails, Is.EqualTo("4411@33.ru, hffty@jhg.ru, 11@33.ru, hgf@jhgj.ut"));
		}
	}
}
