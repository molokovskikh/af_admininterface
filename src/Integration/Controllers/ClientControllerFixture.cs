﻿using System.Collections.Generic;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Functional.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class ClientControllerFixture : BaseControllerTest
	{
		private ClientController controller;
		private Client client;

		[SetUp]
		public void SetUp()
		{
			controller = new ClientController();

			var admin = new Administrator {
				UserName = "TestAdmin",
				RegionMask = 1,
				AllowedPermissions = new List<Permission> {
					new Permission {Type = PermissionType.ChangePassword},
					new Permission {Type = PermissionType.ViewSuppliers},
					new Permission {Type = PermissionType.ViewDrugstore},
				},
			};
			PrepareController(controller);
			SecurityContext.GetAdministrator = () => admin;

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

/*		[Test]
		public void ShowUsersPermisions()
		{
			using (var user = TestUser())
			{
				using (new SessionScope())
					_controller.ShowUsersPermissions(user.Parameter.Client.Id);

				Assert.That(((Client)ControllerContext.PropertyBag["Client"]).Id, Is.EqualTo(user.Parameter.Client.Id));
				Assert.That((new ListMapper((ICollection)ControllerContext.PropertyBag["Permissions"])).Property("Id"),
				            Is.EquivalentTo(new ListMapper(UserPermission.FindPermissionsAvailableFor(user.Parameter.Client)).Property("Id")));
			}
		}*/

/*		[Test]
		[ExpectedException(typeof(NotHavePermissionException))]
		public void Check_permission_on_show_users_permissions()
		{
			SecurityContext.GetAdministrator = () => new Administrator { AllowedPermissions = new List<Permission>() };
			using (var user = TestUser())
				using (new SessionScope())
					_controller.ShowUsersPermissions(user.Parameter.Client.Id);
		}*/

/*		[Test]
		public void Update_user_permissions()
		{
			using (var user = TestUser())
			{
				using (new SessionScope())
				{
					var permissions = UserPermission.FindPermissionsAvailableFor(user.Parameter.Client);
					user.Parameter.AssignedPermissions = new List<UserPermission> {permissions[0]};
					_controller.UpdateUsersPermissions(user.Parameter.Client.Id, new [] { user.Parameter });
				}
			}
		}*/

		[Test]
		public void Move_last_user_to_another_client()
		{
			Client oldClient;
			Client newClient;
			Address address;
			User oldUser;

			using (new SessionScope())
			{
				oldClient = DataMother.CreateTestClientWithAddressAndUser();
				oldUser = oldClient.Users[0];
				address = oldClient.Addresses[0];
				oldUser.AvaliableAddresses = new List<Address>();
				address.AvaliableForUsers.Add(oldUser);
				newClient = DataMother.CreateTestClientWithAddressAndUser();
				
				controller.MoveUserOrAddress(newClient.Id, oldUser.Id, address.Id, newClient.Payer.JuridicalOrganizations[0].Id, false);
			}
			using (new SessionScope())
			{
				oldClient.Refresh();
				newClient.Refresh();
				oldUser.Refresh();
				Assert.That(oldUser.Client.Id, Is.EqualTo(newClient.Id));

				Assert.That(newClient.Users.Count, Is.EqualTo(2));
				Assert.That(oldClient.Users.Count, Is.EqualTo(0));

				Assert.That(newClient.Addresses.Count, Is.EqualTo(1));
				Assert.That(oldClient.Addresses.Count, Is.EqualTo(1));

				Assert.That(oldClient.Status, Is.EqualTo(ClientStatus.On));
			}
		}
	}
}
