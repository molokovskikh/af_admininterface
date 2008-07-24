using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using AdminInterface.Test.Helpers;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Common.Web.Ui.Models;
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
			            		RegionMask = 1,
			            		AllowedPermissions = new List<Permission>
			            		                     	{
			            		                     		new Permission {Type = PermissionType.ChangePassword},
															new Permission{ Type = PermissionType.ViewSuppliers },
															new Permission{ Type = PermissionType.ViewDrugstore},
			            		                     	}

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

		public DisposibleAction<User> TestUser()
		{
			var client = new Client
			             	{
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

			var user = new User { Login = "test" + new Random().Next(), Client = client };
			using (new TransactionScope())
			{
				ActiveRecordMediator.SaveAndFlush(client.ContactGroupOwner);
				client.ContactGroupOwner.ContactGroups[0].ContactGroupOwner = client.ContactGroupOwner;
				client.ContactGroupOwner.ContactGroups[1].ContactGroupOwner = client.ContactGroupOwner;
				client.ContactGroupOwner.ContactGroups[2].ContactGroupOwner = client.ContactGroupOwner;
				ActiveRecordMediator.SaveAndFlush(client.ContactGroupOwner.ContactGroups[0]);
				ActiveRecordMediator.SaveAndFlush(client.ContactGroupOwner.ContactGroups[1]);
				ActiveRecordMediator.SaveAndFlush(client.ContactGroupOwner.ContactGroups[2]);
				ActiveRecordMediator.SaveAndFlush(client);
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
