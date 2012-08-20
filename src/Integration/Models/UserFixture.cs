using System;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Tools;
using Integration.ForTesting;
using Test.Support.log4net;
using log4net.Config;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class UserFixture : Test.Support.IntegrationFixture
	{
		private User user;
		private Client client;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users[0];
			Flush();
		}

		[
			Test,
			ExpectedException(typeof(CantChangePassword), ExpectedMessage = "Не возможно изменить пароль для учетной записи test546116879 поскольку она принадлежит пользователю из офиса"),
			Ignore("Сломан из-за зажатого доступа")
		]
		public void Throw_cant_change_password_exception_if_user_from_office()
		{
			using (var testUser = new TestADUser("test546116879", "LDAP://OU=Офис,DC=adc,DC=analit,DC=net")) {
				user.CheckLogin();
			}
		}

		[Test]
		[ExpectedException(typeof(LoginNotFoundException), ExpectedMessage = "Учетная запись test546116879 не найдена"),
		 Ignore("Не работает, т.к. нет доступа к AD")]
		public void Throw_not_found_exception_if_login_not_exists()
		{
			ADHelper.Delete(user.Login);
			user.CheckLogin();
		}

		[Test]
		public void IsPermissionAssignedTest()
		{
			var permission = new UserPermission { Shortcut = "AF" };
			Assert.That(user.IsPermissionAssigned(permission), Is.False);
			user.AssignedPermissions.Add(new UserPermission { Shortcut = "AF" });
			Assert.That(user.IsPermissionAssigned(permission));
		}

		[Test]
		public void Is_change_password_by_one_self_return_true_if_last_password_change_done_by_client()
		{
			var entities = PasswordChangeLogEntity.GetByLogin(user.Login, DateTime.MinValue, DateTime.MaxValue);
			if (entities.Count > 0)
				entities.Each(l => session.Delete(l));
			Assert.That(user.IsChangePasswordByOneself(), Is.False);
			Save(new PasswordChangeLogEntity(user.Login, user.Login, Environment.MachineName));
			Assert.That(user.IsChangePasswordByOneself(), Is.True);
			Save(new PasswordChangeLogEntity(user.Login, user.Login, Environment.MachineName) {
				LogTime = DateTime.Now.AddSeconds(10)
			});
		}

		[Test]
		public void Create_user_logs()
		{
			Assert.That(user.Logs, Is.Not.Null);
		}

		[Test]
		public void Check_legal_entity_on_user_move()
		{
			var otherClient = DataMother.TestClient();
			var clien = DataMother.TestClient();
			var exception = Assert.Throws<Exception>(() => user.MoveToAnotherClient(clien, otherClient.Orgs().First()));
			Assert.That(exception.Message, Is.StringContaining("не принадлежит клиенту test"));
		}

		[Test]
		public void Move_user()
		{
			var otherClient = DataMother.TestClient();
			var org = otherClient.Orgs().First();

			user.MoveToAnotherClient(otherClient, org);

			Assert.That(user.Client, Is.EqualTo(otherClient));
			Assert.That(user.RootService, Is.EqualTo(otherClient));
			Assert.That(user.Payer, Is.EqualTo(org.Payer));
			Assert.That(user.InheritPricesFrom, Is.Null);
		}

		[Test]
		public void Ignore_new_prices_for_user()
		{
			client.Settings.IgnoreNewPriceForUser = true;
			var pricesCount = user.GetUserPriceCount();
			var supplier = DataMother.CreateSupplier();
			Save(supplier);

			Flush();

			client.MaintainIntersection();
			var newPricesCount = user.GetUserPriceCount();
			Assert.That(newPricesCount, Is.EqualTo(pricesCount));
		}

		[Test]
		public void Delete_user()
		{
			user.Enabled = false;
			Assert.That(user.CanDelete(session), Is.True);
			Flush();
			user.Delete();
			Flush();
		}

		[Test]
		public void Delete_user_with_orders()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			user.AvaliableAddresses.Add(client.Addresses[0]);
			user.Enabled = false;
			Save(new ClientOrder(user, supplier.Prices[0]));

			Assert.That(user.CanDelete(session), Is.True);
			Flush();
			user.Delete();
			Flush();
		}

		[Test(Description = "Проверяем установку флага ForceReplication при изменении свойства InheritPricesFrom")]
		public void SetReplicationInfo()
		{
			var parent = new User(client);
			client.AddUser(parent);
			parent.Setup();

			var supplier = DataMother.CreateSupplier();
			Save(supplier);

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();

			user.InheritPricesFrom = parent;
			session.SaveOrUpdate(user);

			Flush();

			var info = session.CreateSQLQuery("select ForceReplication from Usersettings.AnalitfReplicationInfo where UserId = :UserId")
				.SetParameter("UserId", user.Id)
				.List<object>()
				.Select(v => Convert.ToBoolean(v))
				.ToList();
			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new[] { true }));
		}

		[Test]
		public void Notify_about_work_region_mask_changes()
		{
			ForTest.InitializeMailer();
			MailMessage message = null;
			ChangeNotificationSender.Sender = ForTest.CreateStubSender(m => message = m);
			ChangeNotificationSender.UnderTest = true;

			Reopen();
			user = session.Load<User>(user.Id);
			user.WorkRegionMask = 3;
			Save(user);
			Close();

			Assert.That(message, Is.Not.Null);
			Assert.That(message.Body, Is.StringEnding(String.Format("Код {0}\r\n"
				+ "Пользователь {1}\r\n"
				+ "Клиент {2}\r\n"
				+ "Изменено 'Регионы работы' Добавлено 'Белгород'\r\n", user.Id, user.Name, client.Name)));
			Assert.That(message.To[0].ToString(), Is.EqualTo("BillingList@analit.net"));
			Assert.That(message.Subject, Is.EqualTo("Изменено поле 'Регионы работы'"));
		}
	}
}