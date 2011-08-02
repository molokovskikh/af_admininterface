using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using TransactionlessSession = Integration.ForTesting.TransactionlessSession;

namespace Integration.Controllers
{
	[TestFixture]
	public class UsersControllerFixture : BaseControllerTest
	{
		private UsersController controller;
		private User user1, user2;
		private Client client;
		private UserUpdateInfo info1, info2;

		private ISessionScope session;

		[SetUp]
		public void Setup()
		{
			session = new TransactionlessSession();
			controller = new UsersController();
			PrepareController(controller, "DoPasswordChange");
		}

		[TearDown]
		public void TearDown()
		{
			if (session != null)
				session.Dispose();
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

			controller.DoPasswordChange(user1.Id, "", false, true, false, "");

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
			var address = new Address {
				Client = client,
				Value = "тестовый адрес"
			};
			client.AddAddress(address);
			var user = new User();
			user.Payer = client1.Payers.First();
			var clientContacts = new[] {
				new Contact{Id = 1, Type = 0, ContactText = "4411@33.ru, hffty@jhg.ru"}};
			var regionSettings = new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}};
			var person = new[] {new Person()};

			controller.Add(user, clientContacts, regionSettings, address, person, "", true, client1.Id, "11@33.ru, hgf@jhgj.ut");	
			session.Flush();

			var logs = PasswordChangeLogEntity.Queryable.Where(l => l.TargetUserName == user.Login).ToList();

 			Assert.That(logs.Count, Is.EqualTo(1));
			Assert.That(logs.Single().SentTo, Is.EqualTo("4411@33.ru, hffty@jhg.ru,11@33.ru, hgf@jhgj.ut"));
			Assert.That(user.Accounting, Is.Not.Null);
		}

		[Test]
		public void Register_user_with_comment()
		{
			client = DataMother.CreateTestClientWithUser();
			var user = new User();
			controller.Add(user, new Contact[0], new[] {
					new RegionSettings {
						Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
					},
				}, new Address(), new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);

			var messages = ClientInfoLogEntity.Queryable.Where(l => l.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Create_user_with_permissions()
		{
			client = DataMother.CreateTestClientWithUser();
			user1 = client.Users[0];
			var permission = new UserPermission();
			permission.Id = 1;
			user1.AddPermission(permission);
			user1.Save();
			Assert.That(user1.AssignedPermissions[0].Id, Is.EqualTo(1));
			Assert.That(user1.AssignedPermissions[0].Type.ToString(), Is.EqualTo("Base"));
			Assert.That(user1.AssignedPermissions[0].AvailableFor.ToString(), Is.EqualTo("Supplier"));
		}

		[Test]
		public void Show_supplier_client()
		{
			var user = DataMother.CreateSupplierUser();
			controller.Edit(user.Id);
		}
	}
}
