using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
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
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Test.Support.log4net;

namespace Integration.Controllers
{
	public class CommonUserControllerFixture : ControllerFixture
	{
		protected UsersController controller;
		private DateTime begin;

		[SetUp]
		public void Setup()
		{
			begin = DateTime.Now;
			controller = new UsersController();
			PrepareController(controller, "DoPasswordChange");
			controller.DbSession = session;
		}

		protected void Prepare()
		{
			Request.Params.Add("user.Name", "Тестовый пользователь");
			Request.Params.Add("address.Id", "0");
		}

		protected User Registred()
		{
			return session.Query<User>().Where(u => u.Registration.RegistrationDate >= begin).ToArray().Last();
		}
	}

	[TestFixture]
	public class UsersControllerFixture : CommonUserControllerFixture
	{
		[Test]
		public void Change_user_password()
		{
			var client = DataMother.CreateClientAndUsers();
			var user1 = client.Users[0];
			var user2 = client.Users[1];
			user1.UserUpdateInfo.AFCopyId = "qwerty";
			session.Save(user1.UserUpdateInfo);
			user2.UserUpdateInfo.AFCopyId = "12345";
			session.Save(user2.UserUpdateInfo);

			controller.DoPasswordChange(user1.Id, "", false, true, false, "");

			Assert.That(user1.UserUpdateInfo.AFCopyId, Is.Empty);
			Assert.That(user2.UserUpdateInfo.AFCopyId, Is.EqualTo("12345"));

			var passwordId = HttpUtility.ParseQueryString(new Uri("http://localhost" + Response.RedirectedTo).Query)["passwordId"];
			Assert.IsNotNull(Context.Session[passwordId]);
		}

		[Test]
		public void Add_user()
		{
			var client = DataMother.CreateTestClientWithUser();
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
			Assert.That(logs.Single().SentTo, Is.EqualTo("11@33.ru, hgf@jhgj.ut, 4411@33.ru, hffty@jhg.ru"));
			Assert.That(user.Accounting, Is.Not.Null);
		}

		[Test]
		public void Show_password_after_registration()
		{
			var client = DataMother.CreateClientAndUsers();
			var regionSettings = new[] {
				new RegionSettings { Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true }
			};
			Prepare();
			controller.Add(new Contact[0], regionSettings, new Person[0], "", false, client.Id, null, null, null);

			Assert.IsTrue(Response.WasRedirected);
			var passwordId = HttpUtility.ParseQueryString(new Uri("http://localhost" + Response.RedirectedTo).Query)["passwordId"];
			var password = Context.Session[passwordId];
			Assert.IsNotNull(password);
			Assert.AreNotEqual(password, passwordId);
		}

		[Test]
		public void Register_user_with_comment()
		{
			var client = DataMother.CreateTestClientWithUser();
			Prepare();
			Request.Params.Add("user.Payer.Id", client.Payers.First().Id.ToString());
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null, null, null);

			var user = Registred();
			var messages = session.Query<AuditRecord>().Where(l => l.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "Сообщение в биллинг: тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Register_user_for_client_with_multyplay_payers()
		{
			var client = DataMother.CreateTestClientWithUser();
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
			var client = DataMother.CreateTestClientWithUser();
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
			var client = DataMother.CreateTestClientWithUser();
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

		[Test]
		public void Show_supplier_client()
		{
			var user = DataMother.CreateSupplierUser();
			Flush();

			controller.Edit(user.Id, new MessageQuery());
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
				new[] {
					new Contact {
						ContactText = "123"
					}
				},
				new[] {
					new Contact {
						ContactText = "1231"
					}
				},
				new[] {
					new Person {
						Name = "321"
					}
				},
				new[] {
					new Person {
						Name = "4321"
					}
				});
			Assert.That(user.OrderRegionMask, Is.EqualTo(oldMask));
		}

		[Test]
		public void Reset_cost_on_edit()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			user.Accounting.BeAccounted = true;
			user.Accounting.Payment = 500;
			session.Flush();

			user.SubmitOrders = true;
			user.IgnoreCheckMinOrder = true;
			controller.SaveSettings(user,
				Region.GetRegionsByMask(user.WorkRegionMask).Select(r => r.Id).ToArray(),
				Region.GetRegionsByMask(user.OrderRegionMask).Select(r => r.Id).ToArray());

			Assert.IsTrue(user.FirstTable);
			Assert.IsFalse(user.Accounting.BeAccounted);
			Assert.AreEqual(0, user.Accounting.Payment);
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
	}
}
