using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;
using AdminInterface.Models.Logs;

namespace Integration.Controllers
{
	[TestFixture]
	public class ClientControllerFixture : ControllerFixture
	{
		private ClientsController controller;
		private Client client;

		[SetUp]
		public void SetUp()
		{
			controller = new ClientsController();
			referer = "http://ya.ru";
			PrepareController(controller);
			client = DataMother.CreateTestClientWithUser();
		}


/*		[Test]
		public void Throw_not_found_exception_if_login_not_exists()
		{
			using (var testUser = TestUser())
			using (new SessionScope())
			{
				var login = testUser.Parameter.Login;
				try
				{
					_controller.ChangePassword(testUser.Parameter.Id);
					Assert.Fail("Должны были выбросить исключение");
				}
				catch (Exception ex)
				{
					if (!(ex is LoginNotFoundException))
						throw;
					Assert.That(ex.Message, Is.EqualTo(String.Format("Пользователь {0} не найден", login)));
				}

				try
				{
					_controller.DoPasswordChange(testUser.Parameter.Id, "", false, true, "");
					Assert.Fail("Должны были выбросить исключение");
				}
				catch (Exception ex)
				{
					if (!(ex is LoginNotFoundException))
						throw;
					Assert.That(ex.Message, Is.EqualTo(String.Format("Пользователь {0} не найден", login)));
				}
			}
		}

		[Test, Ignore("нет доступа к ad")]
		public void Throw_cant_change_password_exception_if_user_from_office()
		{
			using (var testUser = TestUser())
			using (var testADUser = new TestADUser(testUser.Parameter.Login, "LDAP://OU=Офис,DC=adc,DC=analit,DC=net"))
			using (new SessionScope())
			{
				var login = testUser.Parameter.Login;

				try
				{
					_controller.ChangePassword(testUser.Parameter.Id);
					Assert.Fail("Должны были выбросить исключение");
				}
				catch (Exception ex)
				{
					if (!(ex is CantChangePassword))
						throw;
				}

				try
				{
					_controller.DoPasswordChange(testUser.Parameter.Id, "", false, true, "");
					Assert.Fail("Должны были выбросить исключение");
				}
				catch (Exception ex)
				{
					if (!(ex is CantChangePassword))
						throw;
				}
			}
		}

		[Test, Ignore("нет доступа к ad")]
		public void Log_password_change()
		{
			using(var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = @"delete from logs.passwordchange where logtime > curdate()";
				command.ExecuteNonQuery();
			}

			using (new SessionScope())
			using (var testAdUser = new TestADUser())
			using (var testUser = TestUser(testAdUser.Login))
			{
				_controller.DoPasswordChange(testUser.Parameter.Id, "r.kvasov@analit.net", true, false, "");
				var passwordChanges = PasswordChangeLogEntity.FindAll(Expression.Gt("LogTime", DateTime.Today));

				Assert.That(passwordChanges.Count(), Is.EqualTo(1));
				//не работает тк антивирус задерживает отправку писем
				//Assert.That(passwordChanges[0].SmtpId, Is.GreaterThan(0));
				Assert.That(passwordChanges[0].SentTo, Is.EqualTo("r.kvasov@analit.net"));
			}
		}
*/
		[Test, Ignore("нет доступа к ad")]
		public void Unlock_every_locked_login()
		{
			using (var adUser1 = new TestADUser())
			using (var adUser2 = new TestADUser())
			{
				client.Users.Add(new User(client) {Login = adUser1.Login});
				client.Users[0].Login = adUser1.Login;
				client.Save();

				using (new SessionScope())
					controller.Unlock(client.Id);

				Assert.That(ADHelper.IsLocked(adUser1.Login), Is.False);
				Assert.That(ADHelper.IsLocked(adUser2.Login), Is.False);
				Assert.That(Response.RedirectedTo, Is.EqualTo("/Controller/Info.castle?cc=" + client.Id));
				Assert.That(Context.Flash["UnlockMessage"], Is.EqualTo("Разблокировано"));
			}
		}

		[Test, Ignore("нет доступа к ad")]
		public void If_login_not_exists_it_must_be_skiped()
		{
			using (var adUser1 = new TestADUser())
			{

				ADHelper.Block(adUser1.Login);

				ActiveRecordMediator.SaveAndFlush(new User(client) { Login = "test8779546" });

				using (new SessionScope())
					controller.Unlock(client.Id);

				Assert.That(ADHelper.IsLocked(adUser1.Login), Is.False);
			}
		}

		[Test, Ignore("нет доступа к ad")]
		[ExpectedException(typeof(NotHavePermissionException))]
		public void Before_unlock_user_permission_must_be_checked()
		{
			SecurityContext.GetAdministrator = () => new Administrator { AllowedPermissions = new List<Permission>() };
			using (var adUser1 = new TestADUser())
			{
				client.Users[0].Login = adUser1.Login;
				client.Save();

				ADHelper.Block(adUser1.Login);

				using (new SessionScope())
					controller.Unlock(client.Id);

				Assert.That(ADHelper.IsLocked(adUser1.Login), Is.False);
			}
		}

		[Test]
		public void Move_last_user_to_another_client()
		{
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var oldUser = oldClient.Users[0];
			var address = oldClient.Addresses[0];
			oldUser.AvaliableAddresses = new List<Address>();
			address.AvaliableForUsers.Add(oldUser);
			var newClient = DataMother.CreateTestClientWithAddressAndUser();

			controller.MoveUserOrAddress(newClient.Id, oldUser.Id, address.Id, newClient.Orgs().First().Id, false);

			oldClient.Refresh();
			newClient.Refresh();
			ActiveRecordMediator.Refresh(oldUser);
			Assert.That(oldUser.Client.Id, Is.EqualTo(newClient.Id));

			Assert.That(newClient.Users.Count, Is.EqualTo(2));
			Assert.That(oldClient.Users.Count, Is.EqualTo(0));

			Assert.That(newClient.Addresses.Count, Is.EqualTo(1));
			Assert.That(oldClient.Addresses.Count, Is.EqualTo(1));

			Assert.That(oldClient.Status, Is.EqualTo(ClientStatus.On));
			Assert.That(notifications.FirstOrDefault(m => m.Subject.Contains("Перемещение пользователя")),
				Is.Not.Null, "не могу найти уведомление о перемещении");
		}

		[Test]
		public void Move_user_with_logs()
		{
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var user = oldClient.Users[0];
			var address = oldClient.Addresses[0];
			user.AvaliableAddresses = new List<Address>();
			address.AvaliableForUsers.Add(user);
			var newClient = DataMother.CreateTestClientWithAddressAndUser();

			controller.MoveUserOrAddress(newClient.Id, user.Id, address.Id, newClient.Orgs().First().Id, false);

			oldClient.Refresh();
			newClient.Refresh();
			var count = ClientInfoLogEntity.Queryable.Count(l => l.Service == newClient && l.ObjectId == user.Id);

			Assert.That(user.Client.Id, Is.EqualTo(newClient.Id));

			Assert.That(newClient.Users.Count, Is.EqualTo(2));
			Assert.That(oldClient.Users.Count, Is.EqualTo(0));

			Assert.That(count, Is.EqualTo(2));
		}

		[Test]
		public void Move_address()
		{
			var sourceClient = DataMother.CreateTestClientWithAddress();
			var destinationClient = DataMother.TestClient();

			var address = sourceClient.Addresses[0];
			controller.MoveUserOrAddress(destinationClient.Id,
				0u,
				address.Id,
				destinationClient.Orgs().First().Id,
				true);
			destinationClient.Refresh();
			sourceClient.Refresh();

			Assert.That(sourceClient.Addresses.Count, Is.EqualTo(0));
			Assert.That(destinationClient.Addresses.Count, Is.EqualTo(1));
			Assert.That(destinationClient.Addresses[0].Id, Is.EqualTo(address.Id));
			Assert.That(notifications.FirstOrDefault(m => m.Subject.Contains("Перемещение адреса доставки")),
				Is.Not.Null, "не могу найти уведомление о перемещении " + notifications.Select(n => n.Subject).Implode());
		}

		[Test]
		public void Search_suppliers()
		{
			DataMother.CreateSupplier().Save();
			scope.Flush();

			Maintainer.MaintainIntersection(client, client.Orgs().First());
			var suppliers = controller.SearchSuppliers(client.Id, "тест");
			Assert.That(suppliers.Length, Is.GreaterThan(0));
		}

		[Test]
		public void Rename_name_and_full_name_client()
		{
			client.Save();
			
			var legalEntity = client.GetLegalEntity()[0];
			legalEntity.Name = "Name";
			legalEntity.FullName = "FullName";
			legalEntity.Save();
			scope.Flush();

			client.Name = "TestName";
			client.FullName = "TestFullName";

			controller.Update(client);

			scope.Flush();

			Assert.That(legalEntity.Name, Is.EqualTo("TestName"));
			Assert.That(legalEntity.FullName, Is.EqualTo("TestFullName"));
		}
	}
}
