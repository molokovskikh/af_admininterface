using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using AddUser;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using AdminInterface.Queries;
using Castle.ActiveRecord.Framework;
using Castle.Components.Binder;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Test.Support.log4net;

namespace Integration.Controllers
{
	[TestFixture]
	public class UsersControllerFixture : ControllerFixture
	{
		private UsersController controller;
		private User user1, user2;
		private Client client;
		private DateTime begin;

		private string json = "{\"Id\":0,\"Login\":\"testLoginRegister\",\"Enabled\":true,\"Name\":\"testComment\",\"SubmitOrders\":false,\"Auditor\":false,\"WorkRegionMask\":0,\"AuthorizationDate\":null,\"Client\":null,\"RootService\":{\"Name\":\"Протек-15\",\"Id\":5},\"SendWaybills\":false,\"SendRejects\":true,\"ShowSupplierCost\":true,\"InheritPricesFrom\":null,\"Payer\":{\"PayerID\":5,\"Name\": \"testPayer\",\"Reports\":null,\"Contacts\":null,\"ContactGroupOwner\":null},\"AvaliableAddresses\":[],\"ImpersonableUsers\":null,\"AssignedPermissions\":[{\"Id\":27,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":29,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":31,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":33,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":35,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":37,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":39,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":41,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":43,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":45,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false},{\"Id\":47,\"Name\":null,\"Shortcut\":null,\"AvailableFor\":0,\"Type\":0,\"AssignDefaultValue\":false}],\"RegionSettings\":[1,8,0,0,0,0,0,0],\"IsInheritPrices\":false,\"NameOrLogin\":\"testComment\",\"CanViewClientInterface\":true}";

		[SetUp]
		public void Setup()
		{
			begin = DateTime.Now;
			controller = new UsersController();
			PrepareController(controller, "DoPasswordChange");
			controller.DbSession = session;
		}

		[Test]
		public void Change_user_password()
		{
			client = DataMother.CreateClientAndUsers();
			user1 = client.Users[0];
			user2 = client.Users[1];
			user1.UserUpdateInfo.AFCopyId = "qwerty";
			session.Save(user1.UserUpdateInfo);
			user2.UserUpdateInfo.AFCopyId = "12345";
			session.Save(user2.UserUpdateInfo);

			controller.DoPasswordChange(user1.Id, "", false, true, false, "");

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
				new Contact { Id = 1, Type = 0, ContactText = "4411@33.ru, hffty@jhg.ru" }
			};
			var regionSettings = new[] {
				new RegionSettings { Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true }
			};
			var person = new[] { new Person() };
			Prepare();
			Request.Params.Add("user.Payer.Id", client.Payers.First().Id.ToString());

			controller.Add(clientContacts, regionSettings, person, "", true, client1.Id, "11@33.ru, hgf@jhgj.ut", null, null);
			Flush();

			var user = Registred();
			var logs = session.Query<PasswordChangeLogEntity>().Where(l => l.TargetUserName == user.Login).ToList();
			Assert.That(logs.Count, Is.EqualTo(1));
			Assert.That(logs.Single().SentTo, Is.EqualTo("4411@33.ru, hffty@jhg.ru,11@33.ru, hgf@jhgj.ut"));
			Assert.That(user.Accounting, Is.Not.Null);
		}

		[Test]
		public void Register_user_with_comment()
		{
			client = DataMother.CreateTestClientWithUser();
			Prepare();
			Request.Params.Add("user.Payer.Id", client.Payers.First().Id.ToString());
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null, null, null);

			var user = Registred();
			var messages = AuditRecord.Queryable.Where(l => l.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "Сообщение в биллинг: тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Register_user_for_client_with_multyplay_payers()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			session.Save(payer);
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);

			Prepare();
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null, null, null);

			Assert.IsTrue(Response.WasRedirected);
			var user = Registred();
			Assert.That(user.Payer.Id, Is.EqualTo(payer.Id));
		}

		[Test]
		public void NotRegisterUserAndAddressWithOtherPayer()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			session.Save(payer);
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);

			var legalEntity = client.Orgs().First();
			Prepare();
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			Request.Params.Add("address.LegalEntity.Id", legalEntity.Id.ToString());
			Request.Params.Add("address.Value", "новый адрес");
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null, null, null);
			Assert.That(controller.Flash["Message"].ToString(),
				Is.StringContaining("Ошибка регистрации: попытка зарегистрировать пользователя и адрес в различных Плательщиках"));
		}

		[Test]
		public void RegisterUserAndAddressWithSamePayerForMultipayerClient()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			session.Save(payer);
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);

			Prepare();
			var legalEntity = client.Orgs().First(entity => entity.Payer.Id == payer.Id);
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			Request.Params.Add("address.LegalEntity.Id", legalEntity.Id.ToString());
			Request.Params.Add("address.Value", "новый адрес");
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null, null, null);

			Assert.IsTrue(Response.WasRedirected);
			var user = Registred();
			Assert.That(user.Payer, Is.EqualTo(payer));
		}

		private void Prepare()
		{
			Request.Params.Add("user.Name", "Тестовый пользователь");
			Request.Params.Add("address.Id", "0");
		}

		[Test]
		public void Show_supplier_client()
		{
			var user = DataMother.CreateSupplierUser();
			Flush();

			controller.Edit(user.Id, new MessageQuery());
		}

		private User Registred()
		{
			return session.Query<User>().Where(u => u.Registration.RegistrationDate >= begin).ToArray().Last();
		}

		[Test]
		public void NoChangeOrderRegionIfSupplierTest()
		{
			var user = DataMother.CreateSupplierUser();
			user.Name = "Тестовый пользователь для редактирования";
			user.OrderRegionMask = 16;
			session.Save(user);
			var oldMask = user.OrderRegionMask;
			controller.Update(user,
				new Contact[1] {
					new Contact {
						ContactText = "123"
					}
				},
				new Contact[1] {
					new Contact {
						ContactText = "1231"
					}
				},
				new Person[1] {
					new Person {
						Name = "321"
					}
				},
				new Person[1] {
					new Person {
						Name = "4321"
					}
				});
			Assert.That(user.OrderRegionMask, Is.EqualTo(oldMask));
		}

		[Test]
		public void Search_user_for_show_user_test()
		{
			var client = DataMother.CreateClientAndUsers();
			client.Name = Generator.Name();
			var user = client.Users.First();
			user.Login = Generator.Name();
			session.Save(user);
			session.Save(client);

			Flush();
			var loginObj = controller.SearchForShowUser(user.Login.Substring(0, 5));
			Assert.AreEqual(loginObj.Count(), 1);

			var clientObj = controller.SearchForShowUser(client.Name.Substring(0, 5));
			Assert.AreEqual(clientObj.Count(), 2);
		}

		[Test]
		public void Reset_af_version_test()
		{
			var client = DataMother.CreateClientAndUsers();
			var user = client.Users[0];
			controller.ResetAFVersion(user.Id);
			Flush();
			Assert.AreEqual(session.Get<User>(user.Id).UserUpdateInfo.AFAppVersion, 999);
		}

		[Test]
		public void CollectionFromJSonText()
		{
			var user = new User();
			controller.BindObjectInstanceForUser(user, "User", json);

			var regionMask = BitConverter.ToUInt64(user.RegionSettings.Select(r => Convert.ToByte(r)).ToArray(), 0);

			Assert.AreEqual(regionMask, 2049);
			Assert.AreEqual(user.Login, "testLoginRegister");
			Assert.AreEqual(user.Name, "testComment");
			Assert.AreEqual(user.AssignedPermissions.Count, 11);
			Assert.AreEqual(user.AssignedPermissions[0].Id, 27);
			Assert.AreEqual(user.RootService.Id, 5);
			Assert.AreEqual(user.RootService.Name, "Протек-15");
			Assert.IsNull(user.InheritPricesFrom);
		}

		[Test]
		public void Patch_json()
		{
			var result = UsersController.PatchJson(json);
			Assert.That(result, Is.Not.StringContaining("PayerID"));
		}

		[Test]
		public void Add_from_json()
		{
			var tempLogin = Generator.Name();
			var supplier = DataMother.CreateSupplier();
			session.Save(supplier);

			json = json.Replace("\"Id\":5", string.Format("\"Id\":{0}", supplier.Id)).Replace("\"PayerID\":5", string.Format("\"PayerID\":{0}", supplier.Payer.Id)).Replace("testLoginRegister", tempLogin);

			Prepare();
			PrepareController(controller);

			Flush();
			controller.Add();

			var user = Registred();

			Assert.AreEqual(user.Login, tempLogin);
			Assert.AreEqual(user.Name, "testComment");
		}

		protected override IMockRequest BuildRequest()
		{
			var requestTest = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var request = MockRepository.GenerateStub<IMockRequest>();
			request.Stub(r => r.InputStream).Return(requestTest);
			request.Stub(r => r.Params).Return(new NameValueCollection());
			request.Stub(r => r.ObtainParamsNode(ParamStore.Params)).Return(new CompositeNode("root"));
			return request;
		}
	}
}
