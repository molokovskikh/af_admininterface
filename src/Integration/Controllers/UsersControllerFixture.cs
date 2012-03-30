using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class UsersControllerFixture : ControllerFixture
	{
		private UsersController controller;
		private User user1, user2;
		private Client client;
		private DateTime begin;

		[SetUp]
		public void Setup()
		{
			begin = DateTime.Now;
			controller = new UsersController();
			PrepareController(controller, "DoPasswordChange");
		}

		[Test]
		public void Change_user_password()
		{
			client = DataMother.CreateClientAndUsers();
			user1 = client.Users[0];
			user2 = client.Users[1];
			user1.UserUpdateInfo.AFCopyId = "qwerty";
			user1.UserUpdateInfo.Save();
			user2.UserUpdateInfo.AFCopyId = "12345";
			user2.UserUpdateInfo.Save();

			controller.DoPasswordChange(user1.Id, "", false, true, false, "");

			user1.UserUpdateInfo.Refresh();
			user2.UserUpdateInfo.Refresh();
			Assert.That(user1.UserUpdateInfo.AFCopyId, Is.Empty);
			Assert.That(user2.UserUpdateInfo.AFCopyId, Is.EqualTo("12345"));
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
			var clientContacts = new[] {
				new Contact{Id = 1, Type = 0, ContactText = "4411@33.ru, hffty@jhg.ru"}};
			var regionSettings = new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true}};
			var person = new[] {new Person()};
			Prepare();

			controller.Add(clientContacts, regionSettings, address, person, "", true, client1.Id, "11@33.ru, hgf@jhgj.ut");
			scope.Flush();

			var user = Registred();
			var logs = PasswordChangeLogEntity.Queryable.Where(l => l.TargetUserName == user.Login).ToList();
			Assert.That(logs.Count, Is.EqualTo(1));
			Assert.That(logs.Single().SentTo, Is.EqualTo("4411@33.ru, hffty@jhg.ru,11@33.ru, hgf@jhgj.ut"));
			Assert.That(user.Accounting, Is.Not.Null);
		}

		[Test]
		public void Register_user_with_comment()
		{
			client = DataMother.CreateTestClientWithUser();
			Prepare();
			controller.Add(new Contact[0], new[] {
					new RegionSettings {
						Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
					},
				}, new Address(), new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);

			var user = Registred();
			var messages = ClientInfoLogEntity.Queryable.Where(l => l.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "Сообщение в биллинг: тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Register_user_for_client_with_multyplay_payers()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			payer.Save();
			client.Payers.Add(payer);
			client.Save();
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			controller.Add(new Contact[0], new[] {
					new RegionSettings {
						Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
					},
				}, new Address(), new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);

			var user = Registred();
			Assert.That(user.Payer, Is.EqualTo(payer));
		}

		private void Prepare()
		{
			Request.Params.Add("user.Name", "Тестовый пользователь");
		}

		[Test]
		public void Create_user_with_permissions()
		{
			client = DataMother.CreateTestClientWithUser();
			scope.Flush();
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
			scope.Flush();

			controller.Edit(user.Id);
		}

		private User Registred()
		{
			return ActiveRecordLinqBase<User>.Queryable.Where(u => u.RegistrationDate >= begin).ToArray().Last();
		}
	}
}
