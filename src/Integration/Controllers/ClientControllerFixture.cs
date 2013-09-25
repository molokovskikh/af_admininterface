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
using NHibernate.Linq;
using Test.Support.log4net;
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
			Prepare(controller);
			client = DataMother.CreateTestClientWithUser();
		}

		[Test]
		public void NotifySuppliersTest()
		{
			controller.NotifySuppliers(client.Id);
			var objectType = AuditRecord.GetLogObjectType(client);
			var audit = session.Query<AuditRecord>().Where(a => a.ObjectId == client.Id && a.Type == objectType).ToList();
			Assert.IsTrue(audit.Any(a => a.Message.Contains("Разослано повторное уведомление о регистрации клиента")));
		}

		[Test, Ignore("нет доступа к ad")]
		public void Unlock_every_locked_login()
		{
			using (var adUser1 = new TestADUser())
				using (var adUser2 = new TestADUser()) {
					client.Users.Add(new User(client) { Login = adUser1.Login });
					client.Users[0].Login = adUser1.Login;
					session.SaveOrUpdate(client);

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
			using (var adUser1 = new TestADUser()) {
				ADHelper.Block(adUser1.Login);

				session.SaveOrUpdate(new User(client) { Login = "test8779546" });

				controller.Unlock(client.Id);

				Assert.That(ADHelper.IsLocked(adUser1.Login), Is.False);
			}
		}

		[Test, Ignore("нет доступа к ad")]
		[ExpectedException(typeof(NotHavePermissionException))]
		public void Before_unlock_user_permission_must_be_checked()
		{
			SecurityContext.GetAdministrator = () => new Administrator { AllowedPermissions = new List<Permission>() };
			using (var adUser1 = new TestADUser()) {
				client.Users[0].Login = adUser1.Login;
				session.SaveOrUpdate(client);

				ADHelper.Block(adUser1.Login);

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
			session.Flush();

			session.Refresh(oldClient);
			session.Refresh(newClient);
			session.Refresh(oldUser);
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
			session.Flush();

			session.Refresh(oldClient);
			session.Refresh(newClient);
			var records = session.Query<AuditRecord>()
				.Where(l => l.Service == newClient && l.ObjectId == user.Id)
				.ToList();

			Assert.That(user.Client.Id, Is.EqualTo(newClient.Id));

			Assert.That(newClient.Users.Count, Is.EqualTo(2));
			Assert.That(oldClient.Users.Count, Is.EqualTo(0));

			Assert.That(records.Count, Is.EqualTo(3), records.Implode());
			Assert.That(records.Implode(), Is.StringContaining("Перемещение пользователя от"));
		}

		[Test]
		public void Move_address()
		{
			var src = DataMother.CreateTestClientWithAddress();
			var dst = DataMother.TestClient();
			var address = src.Addresses[0];

			//должны быть уникальные имена что можно было сделать проверку на правильность написания письма
			MakeNameUniq(src);
			MakeNameUniq(src.Payers);
			MakeNameUniq(src.Payers.SelectMany(p => p.Orgs));

			MakeNameUniq(dst);
			MakeNameUniq(dst.Payers);
			MakeNameUniq(dst.Payers.SelectMany(p => p.Orgs));

			controller.MoveUserOrAddress(dst.Id,
				0u,
				address.Id,
				dst.Orgs().First().Id,
				true);
			session.Flush();

			session.Refresh(dst);
			session.Refresh(src);

			Assert.That(src.Addresses.Count, Is.EqualTo(0));
			Assert.That(src.Disabled, Is.True);
			Assert.That(dst.Addresses.Count, Is.EqualTo(1));
			Assert.That(dst.Addresses[0].Id, Is.EqualTo(address.Id));
			var mail = notifications.FirstOrDefault(m => m.Subject.Contains("Перемещение адреса доставки"));
			Assert.That(mail,
				Is.Not.Null, "не могу найти уведомление о перемещении " + notifications.Select(n => n.Subject).Implode());
			Assert.That(mail.Body, Is.StringContaining(String.Format("Старый клиент {0} плательщик {1} юр.лицо {2}",
				src.Name,
				src.Payers[0].Name,
				src.Payers[0].Orgs[0].Name)));
			Assert.That(mail.Body, Is.StringContaining(String.Format("Новый клиент {0} плательщик {1} юр.лицо {2}",
				dst.Name,
				dst.Payers[0].Name,
				dst.Payers[0].Orgs[0].Name)));
		}

		[Test]
		public void Search_suppliers()
		{
			session.Save(DataMother.CreateSupplier());
			Flush();

			Maintainer.MaintainIntersection(client, client.Orgs().First());
			var suppliers = controller.SearchSuppliers(client.Id, "тест");
			Assert.That(suppliers.Length, Is.GreaterThan(0));
		}

		[Test]
		public void Rename_name_and_full_name_client()
		{
			session.SaveOrUpdate(client);

			var legalEntity = client.GetLegalEntity()[0];
			legalEntity.Name = "Name";
			legalEntity.FullName = "FullName";
			session.Save(legalEntity);
			Flush();

			client.Name = "TestName";
			client.FullName = "TestFullName";

			controller.Update(client);

			Flush();

			Assert.That(legalEntity.Name, Is.EqualTo("TestName"));
			Assert.That(legalEntity.FullName, Is.EqualTo("TestFullName"));
		}
	}
}