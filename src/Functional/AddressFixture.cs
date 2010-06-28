using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using System.Collections;
using Common.Web.Ui.Models;
using System.Threading;

namespace Functional
{
	public class AddressFixture : WatinFixture
	{
		[Test]
		public void DeleteContactInformation()
		{
			var client = DataMother.CreateTestClientWithUser();
			var countContacts = 0;
			// Удаление контактной записи
			using (var browser = Open("client/{0}", client.Id))
			{
				countContacts = AddContactsToNewDeliveryAddress(browser, client.Id);
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Addresses[0].ContactGroup;
					browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
					Thread.Sleep(500);
					browser.Button(Find.ByValue("Сохранить")).Click();
				}
			}
			Thread.Sleep(500);
			// Проверка, что контактная запись удалена
			var count = ContactInformationFixture.GetCountContactsInDb(client.Addresses[0].ContactGroup);
			Assert.That(count, Is.EqualTo(countContacts - 1));
		}

		[Test]
		public void Create_address()
		{
			var client = DataMother.CreateTestClient();

			using(var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Новый адрес доставки")).Click();
				Assert.That(browser.Text, Is.StringContaining("Новый адрес доставки"));
				browser.TextField(Find.ByName("delivery.value")).TypeText("тестовый адрес");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Адрес доставки создан"));
			}

			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.That(client.Addresses.Count, Is.EqualTo(1), "не создали адресс доставки");
				var address = client.Addresses.First();
				Assert.That(address.Value, Is.EqualTo("тестовый адрес"));

				var intersection = ArHelper.WithSession(s =>
					s.CreateSQLQuery("select * from future.Intersection where ClientId = :id")
					.SetParameter("id", client.Id)
					.List());

				var addressIntersection = ArHelper.WithSession(s =>
					s.CreateSQLQuery("select * from future.AddressIntersection where AddressId = :id")
					.SetParameter("id", address.Id)
					.List());

				Assert.That(intersection.Count, Is.GreaterThan(0), "не создали записей в Intersection, проверрь создание пользователя Client.MaintainIntersection");
				Assert.That(addressIntersection.Count, Is.GreaterThan(0), "не создали записей в AddressIntersection, адрес не будет доступен в клиентском интерфейсе");
				Assert.That(addressIntersection.Count, Is.EqualTo(intersection.Count), "Не совпадает число записей в Intersection и AddressIntersection проверь Address.MaintainIntersection");
			}
		}

		[Test]
		public void Send_notification()
		{
			var client = DataMother.CreateTestClientWithAddress();
			using (var browser = Open("deliveries/{0}/edit", client.Addresses.First().Id))
			{
				Assert.That(browser.Text, Is.StringContaining("Адрес доставки"));
				browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
				Assert.That(browser.Text, Is.StringContaining("Уведомления отправлены"));
			}
		}

		// Создает новый адрес доставки и 3 контакта для него (2 email)
		private int AddContactsToNewDeliveryAddress(IE browser, uint clientId)
		{
			var applyButtonText = "Создать";
			browser.Link(Find.ByText("Новый адрес доставки")).Click();
			browser.TextField(Find.ByName("delivery.value")).TypeText("Test address");
			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, clientId);
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки создан"));
			browser.Link(Find.ByText("Test address")).Click();
			applyButtonText = "Сохранить";
			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, clientId);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			browser.Link(Find.ByText("Test address")).Click();
			ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, clientId);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			browser.Link(Find.ByText("Test address")).Click();
			return 3;
		}

		[Test]
		public void AddContactInformation()
		{
			var applyButtonText = "Создать";
			var client = DataMother.CreateTestClient();
			var countContacts = 0;
			using (var browser = Open("client/{0}", client.Id))
			{
				countContacts = AddContactsToNewDeliveryAddress(browser, client.Id);
			}
			// Проверка, что контактные записи создались в БД
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.NotNull(client.Addresses[0].ContactGroup);
				var group = client.Addresses[0].ContactGroup;
				Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
					"Не совпадают Id владельца группы у клиента и у новой группы");
				ContactInformationFixture.CheckContactGroupInDb(group);
				countContacts = ContactInformationFixture.GetCountContactsInDb(group);
				Assert.That(countContacts, Is.EqualTo(countContacts));
			}
		}

		[Test]
		public void Address_must_be_enabled_after_registration()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				using (new SessionScope())
				{
					browser.Link(Find.ByText("Новый адрес доставки")).Click();
					browser.TextField(Find.ByName("delivery.value")).TypeText("test address");
					browser.Button(Find.ByValue("Создать")).Click();
					Assert.That(browser.Text, Text.Contains("Адрес доставки создан"));
					client = Client.Find(client.Id);
					Assert.That(client.Addresses.Count, Is.EqualTo(1));
					Assert.IsTrue(client.Addresses[0].Enabled);
				}
			}
		}

		[Test, Description("Перемещение адреса вместе с пользователем к другому клиенту")]
		public void Move_address_with_user_to_another_client()
		{
			Client oldClient;
			Client newClient;
			Address address;
			User user;

			using (new SessionScope())
			{
				newClient = DataMother.CreateTestClientWithAddressAndUser();
				oldClient = DataMother.CreateTestClientWithAddressAndUser();
				address = oldClient.Addresses[0];
				user = oldClient.Users[0];
			}

			using (var browser = Open("Deliveries/{0}/Edit", address.Id))
			{
				// Даем доступ пользователю к адресу доставки
				browser.CheckBox(Find.ByName("delivery.AvaliableForUsers[0].Id")).Checked = true;
				browser.Button(Find.ByValue("Сохранить")).Click();
				browser.Refresh();

				browser.Link(Find.ByText(address.Value)).Click();
				// Ищем клиента, к которому нужно передвинуть пользователя и двигаем
				browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
				browser.Button(Find.ById("SearchClientButton")).Click();
				Thread.Sleep(2000);
				Assert.That(browser.Text, Is.StringContaining(String.Format("Перемещать пользователя {0}", user.Id)));
				Assert.IsTrue(browser.CheckBox(Find.ByName("moveWithUser")).Checked);
				browser.Button(Find.ByValue("Переместить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Адрес доставки успешно перемещен"));
			}

			using (new SessionScope())
			{
				oldClient.Refresh();
				newClient.Refresh();
				address.Refresh();
				Assert.That(address.Client.Id, Is.EqualTo(newClient.Id));

				Assert.That(newClient.Users.Count, Is.EqualTo(2));
				Assert.That(oldClient.Users.Count, Is.EqualTo(0));

				Assert.That(newClient.Addresses.Count, Is.EqualTo(2));
				Assert.That(oldClient.Addresses.Count, Is.EqualTo(0));
			}
		}

				[Test, Description("Перемещение только адреса доставки (без пользователя) к другому клиенту")]
		public void Move_only_address_to_another_client()
		{
			Client oldClient;
			Client newClient;
			uint addressIdForMove = 0;
			using (new SessionScope())
			{
				oldClient = DataMother.CreateTestClientWithAddressAndUser();
				newClient = DataMother.CreateTestClientWithAddressAndUser();
				addressIdForMove = oldClient.Addresses[0].Id;
			}

			using (var browser = Open("deliveries/{0}/edit", oldClient.Addresses[0].Id))
			{
				browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
				browser.Button(Find.ById("SearchClientButton")).Click();
				Thread.Sleep(2000);
				Assert.IsTrue(browser.SelectList(Find.ById("clientsList")).Exists);
				Assert.That(browser.SelectList(Find.ById("clientsList")).Options.Length, Is.GreaterThan(0));

				Assert.IsTrue(browser.Button(Find.ByValue("Отмена")).Exists);
				Assert.IsTrue(browser.Button(Find.ByValue("Переместить")).Exists);

				browser.Button(Find.ByValue("Переместить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Адрес доставки успешно перемещен"));
				Assert.That(browser.Text, Is.StringContaining(newClient.Name));
				Assert.That(browser.Text, Is.Not.StringContaining(oldClient.Name));
			}

			using (new SessionScope())
			{
				oldClient.Refresh();
				newClient.Refresh();
				var address = Address.Find(addressIdForMove);
				Assert.That(address.Client.Id, Is.EqualTo(newClient.Id));
				Assert.That(newClient.Addresses.Count, Is.EqualTo(2));
				Assert.That(oldClient.Addresses.Count, Is.EqualTo(0));
			}
		}
	}
}
