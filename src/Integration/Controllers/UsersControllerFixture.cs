using System;
using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using log4net.Config;
using NHibernate;
using NUnit.Framework;

namespace Integration.Controllers
{
	public class TransactionlessSession : AbstractScope
	{
		public TransactionlessSession()
			: base(FlushAction.Auto, SessionScopeType.Simple)
		{}

		public override void FailSession(ISession session)
		{}

		public override ISession GetSession(object key)
		{
			return key2Session[key];
		}

		public override ISession OpenSession(ISessionFactory sessionFactory, IInterceptor interceptor)
		{
			var session = sessionFactory.OpenSession(interceptor);
			return session;
		}
	}

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

			controller.Add(user, clientContacts, regionSettings, address, person, "", true, client1.Id, "11@33.ru, hgf@jhgj.ut");	

			var mails = ArHelper.WithSession(s => s
				.CreateSQLQuery(@"select sentto from logs.passwordchange where targetusername = :userId")
			    .SetParameter("userId", user.Id)
			    .UniqueResult());
 
			Assert.That(mails, Is.EqualTo("4411@33.ru, hffty@jhg.ru,11@33.ru, hgf@jhgj.ut"));
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
