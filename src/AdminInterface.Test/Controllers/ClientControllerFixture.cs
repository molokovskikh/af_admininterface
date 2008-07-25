using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AddUser;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using AdminInterface.Test.Helpers;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;
using NHibernate.Criterion;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Controllers
{
	[TestFixture]
	public class ClientControllerFixture : BaseControllerTest
	{
		private ClientController _controller;

		[SetUp]
		public void SetUp()
		{
			_controller = new ClientController();
			var admin = new Administrator
			            	{
								UserName = "TestAdmin",
			            		RegionMask = 1,
			            		AllowedPermissions = new List<Permission>
			            		                     	{
			            		                     		new Permission {Type = PermissionType.ChangePassword},
															new Permission{ Type = PermissionType.ViewSuppliers },
															new Permission{ Type = PermissionType.ViewDrugstore},
			            		                     	},
			            	};
			PrepareController(_controller);
			SecurityContext.GetAdministrator = () => admin;
			ForTest.InitialzeAR();
		}

		[Test]
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
					Assert.That(ex.Message, Is.EqualTo(String.Format("Учетная запись {0} не найдена", login)));
				}

				try
				{
					_controller.DoPasswordChange(testUser.Parameter.Id, "", false, true, "", "");
					Assert.Fail("Должны были выбросить исключение");
				}
				catch (Exception ex)
				{
					if (!(ex is LoginNotFoundException))
						throw;
					Assert.That(ex.Message, Is.EqualTo(String.Format("Учетная запись {0} не найдена", login)));
				}
			}
		}

		[Test]
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
					_controller.DoPasswordChange(testUser.Parameter.Id, "", false, true, "", "");
					Assert.Fail("Должны были выбросить исключение");
				}
				catch (Exception ex)
				{
					if (!(ex is CantChangePassword))
						throw;
				}
			}
		}

		[Test]
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
				_controller.DoPasswordChange(testUser.Parameter.Id, "r.kvasov@analit.net", true, false, "", "");
				var passwordChanges = PasswordChangeLogEntity.FindAll(Expression.Gt("LogTime", DateTime.Today));

				Assert.That(passwordChanges.Count(), Is.EqualTo(1));
				//не работает тк антивирус задерживает отправку писем
				//Assert.That(passwordChanges[0].SmtpId, Is.GreaterThan(0));
				Assert.That(passwordChanges[0].SentTo, Is.EqualTo("r.kvasov@analit.net"));
			}
		}

		public DisposibleAction<User> TestUser()
		{
			return TestUser("test" + new Random().Next());
		}

		public DisposibleAction<User> TestUser(string userName)
		{
			var client = new Client
			             	{
								ShortName = "TestClient",
								FullName = "TestClient",
								BillingInstance = new Payer
								                  	{
								                  		ShortName = "TestPayer",
								                  	},
			             		HomeRegion = Region.Find(1UL),
			             		ContactGroupOwner = new ContactGroupOwner
			             		                    	{
			             		                    		ContactGroups = new List<ContactGroup>
			             		                    		                	{
			             		                    		                		new ContactGroup { Name = "123", Type = ContactGroupType.General },
																					new ContactGroup { Name = "123", Type = ContactGroupType.OrderManagers },
																					new ContactGroup { Name = "123", Type = ContactGroupType.ClientManagers },
			             		                    		                	}
			             		                    	}
			             	};

			var user = new User { Login = userName, Client = client };
			using (new TransactionScope())
			{
				ActiveRecordMediator.Save(client.ContactGroupOwner);
				client.ContactGroupOwner.ContactGroups[0].ContactGroupOwner = client.ContactGroupOwner;
				client.ContactGroupOwner.ContactGroups[1].ContactGroupOwner = client.ContactGroupOwner;
				client.ContactGroupOwner.ContactGroups[2].ContactGroupOwner = client.ContactGroupOwner;
				ActiveRecordMediator.Save(client.ContactGroupOwner.ContactGroups[0]);
				ActiveRecordMediator.Save(client.ContactGroupOwner.ContactGroups[1]);
				ActiveRecordMediator.Save(client.ContactGroupOwner.ContactGroups[2]);
				ActiveRecordMediator.Save(client.BillingInstance);
				ActiveRecordMediator.Save(client);
				ActiveRecordMediator.SaveAndFlush(user);
			}
			return new DisposibleAction<User>(user, () =>
			                                        	{
			                                        		ActiveRecordMediator<User>.Delete(user);
															ActiveRecordMediator<Client>.Delete(client);
			                                        	});
		}
	}

	public class DisposibleAction<T> : IDisposable
	{
		private readonly Action _dispose;

		public DisposibleAction(T t, Action dispose)
		{
			Parameter = t;
			_dispose = dispose;
		}

		public T Parameter { get; private set; }

		public void Dispose()
		{
			_dispose();
		}
	}
}
