using System;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Common.Tools;
using log4net.Config;
using NUnit.Framework;
using Functional.ForTesting;

namespace Integration.Models
{
	[TestFixture]
	public class UserFixture
	{
		private User user;

		[SetUp]
		public void Setup()
		{
			BasicConfigurator.Configure();
			var client = DataMother.CreateTestClientWithUser();
			user = client.Users[0];
		}

		[
			Test,
			ExpectedException(typeof(CantChangePassword), ExpectedMessage = "Не возможно изменить пароль для учетной записи test546116879 поскольку она принадлежит пользователю из офиса"),
			Ignore("Сломан из-за зажатого доступа")
		]
		public void Throw_cant_change_password_exception_if_user_from_office()
		{
			using (var testUser = new TestADUser("test546116879", "LDAP://OU=Офис,DC=adc,DC=analit,DC=net"))
			{
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
			var permission = new UserPermission {Shortcut = "AF"};
			Assert.That(user.IsPermissionAssigned(permission), Is.False);
			user.AssignedPermissions.Add(new UserPermission { Shortcut = "AF"});
			Assert.That(user.IsPermissionAssigned(permission));
		}

		[Test]
		public void Is_change_password_by_one_self_return_true_if_last_password_change_done_by_client()
		{
			var entities = PasswordChangeLogEntity.GetByLogin(user.Login, DateTime.MinValue, DateTime.MaxValue);
			if (entities.Count > 0)
				entities.Each(l => l.Delete());
			Assert.That(user.IsChangePasswordByOneself(), Is.False);
			new PasswordChangeLogEntity(user.Login, user.Login, Environment.MachineName).Save();
			Assert.That(user.IsChangePasswordByOneself(), Is.True);
			new PasswordChangeLogEntity(user.Login, user.Login, Environment.MachineName) {
				LogTime = DateTime.Now.AddSeconds(10)
			}.Save();
		}

		[Test]
		public void Create_user_logs()
		{
			var client = DataMother.CreateTestClientWithUser();
			Assert.That(client.Users[0].Logs, Is.Not.Null);
		}
	}
}
