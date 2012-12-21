﻿using System;
using System.Linq;
using AddUser;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using AdminInterface.Queries;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
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
			user1.UserUpdateInfo.Save();
			user2.UserUpdateInfo.AFCopyId = "12345";
			user2.UserUpdateInfo.Save();

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

			controller.Add(clientContacts, regionSettings, person, "", true, client1.Id, "11@33.ru, hgf@jhgj.ut");
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
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);

			var user = Registred();
			var messages = AuditRecord.Queryable.Where(l => l.ObjectId == user.Id);
			Assert.That(messages.Any(m => m.Message == "Сообщение в биллинг: тестовое сообщение для биллинга"), Is.True, messages.Implode(m => m.Message));
		}

		[Test]
		public void Register_user_for_client_with_multyplay_payers()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			payer.Save();
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);

			var user = Registred();
			Assert.That(user.Payer, Is.EqualTo(payer));
		}

		[Test]
		public void NotRegisterUserAndAddressWithOtherPayer()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			payer.Save();
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);

			var legalEntity = client.Orgs().First();
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			Request.Params.Add("address.LegalEntity.Id", legalEntity.Id.ToString());
			Request.Params.Add("address.Value", "новый адрес");
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);
			Assert.That(controller.Flash["Message"].ToString(),
				Is.StringContaining("Ошибка регистрации: попытка зарегистрировать пользователя и адрес в различных Плательщиках"));
		}

		[Test]
		public void RegisterUserAndAddressWithSamePayerForMultipayerClient()
		{
			client = DataMother.CreateTestClientWithUser();
			var payer = new Payer("Тестовый плательщик");
			payer.Save();
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);

			var legalEntity = client.Orgs().First(entity => entity.Payer.Id == payer.Id);
			Request.Params.Add("user.Payer.Id", payer.Id.ToString());
			Request.Params.Add("address.LegalEntity.Id", legalEntity.Id.ToString());
			Request.Params.Add("address.Value", "новый адрес");
			controller.Add(new Contact[0], new[] {
				new RegionSettings {
					Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true
				},
			}, new Person[0], "тестовое сообщение для биллинга", true, client.Id, null);

			var user = Registred();
			Assert.That(user.Payer, Is.EqualTo(payer));
		}

		private void Prepare()
		{
			Request.Params.Add("user.Name", "Тестовый пользователь");
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
				new ulong[1],
				new ulong[1],
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
			client.Name = "testClientForShowUserFindTest";
			var user = client.Users.First();
			user.Login = Guid.NewGuid().ToString().Replace("-", string.Empty);
			session.Save(user);
			session.Save(client);

			Flush();
			var loginObj = controller.SearchForShowUser(user.Login.Substring(0, 5));
			Assert.AreEqual(loginObj.Count(), 1);

			var clientObj = controller.SearchForShowUser("ForShowUserFind");
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
