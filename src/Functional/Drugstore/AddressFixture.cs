using System;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;
using WatiN.Core;
using Common.Web.Ui.Models;
using System.Threading;

namespace Functional.Drugstore
{
	public class AddressFixture : WatinFixture2
	{
		private Client client;

		[SetUp]
		public void Setup()
		{
			var supplier = DataMother.CreateSupplier();
			supplier.Save();
			client = DataMother.CreateTestClientWithUser();
			scope.Flush();
			Open(client);
			Assert.That(browser.Text, Is.StringContaining("Клиент"));
		}

		[Test]
		public void DeleteContactInformation()
		{
			// Удаление контактной записи
			var countContacts = AddContactsToNewDeliveryAddress(client.Id);
			client.Refresh();
			var group = client.Addresses[0].ContactGroup;
			browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
			Thread.Sleep(500);
			browser.Button(Find.ByValue("Сохранить")).Click();

			// Проверка, что контактная запись удалена
			Thread.Sleep(500);
			var count = ContactInformationFixture.GetCountContactsInDb(client.Addresses[0].ContactGroup);
			Assert.That(count, Is.EqualTo(countContacts - 1));
		}

		[Test]
		public void Create_address()
		{
			browser.Link(Find.ByText("Новый адрес доставки")).Click();
			Assert.That(browser.Text, Is.StringContaining("Новый адрес доставки"));
			browser.TextField(Find.ByName("delivery.value")).TypeText("тестовый адрес");
			browser.Button(Find.ByValue("Создать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки создан"));

			client.Refresh();
			Assert.That(client.Addresses.Count, Is.EqualTo(1), "не создали адресс доставки");
			var address = client.Addresses.First();
			Assert.That(address.Value, Is.EqualTo("тестовый адрес"));

			var intersectionCount = client.GetIntersectionCount();
			var addressIntersectionCount = address.GetAddressIntersectionCount();

			Assert.That(intersectionCount, Is.GreaterThan(0), "не создали записей в Intersection, проверрь создание пользователя Client.MaintainIntersection");
			Assert.That(addressIntersectionCount, Is.GreaterThan(0), "не создали записей в AddressIntersection, адрес не будет доступен в клиентском интерфейсе");
			Assert.That(addressIntersectionCount, Is.EqualTo(intersectionCount), "Не совпадает число записей в Intersection и AddressIntersection проверь Address.MaintainIntersection");
		}

		[Test]
		public void Send_notification()
		{
			var client = DataMother.CreateTestClientWithAddress();
			var address = client.Addresses.First();
			Open(address);
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки"));
			browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
			Assert.That(browser.Text, Is.StringContaining("Уведомления отправлены"));
		}

		// Создает новый адрес доставки и 3 контакта для него (2 email)
		private int AddContactsToNewDeliveryAddress(uint clientId)
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
			var countContacts = AddContactsToNewDeliveryAddress(client.Id);
			// Проверка, что контактные записи создались в БД

			client.Refresh();
			Assert.NotNull(client.Addresses[0].ContactGroup);
			var group = client.Addresses[0].ContactGroup;
			Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
				"Не совпадают Id владельца группы у клиента и у новой группы");

			ContactInformationFixture.CheckContactGroupInDb(group);
			countContacts = ContactInformationFixture.GetCountContactsInDb(group);
			Assert.That(countContacts, Is.EqualTo(countContacts));
		}

		[Test]
		public void Address_must_be_enabled_after_registration()
		{
			browser.Link(Find.ByText("Новый адрес доставки")).Click();
			browser.TextField(Find.ByName("delivery.value")).TypeText("test address");
			browser.Button(Find.ByValue("Создать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки создан"));

			client.Refresh();
			Assert.That(client.Addresses.Count, Is.EqualTo(1));
			Assert.IsTrue(client.Addresses[0].Enabled);
		}

		[Test, NUnit.Framework.Description("Перемещение адреса вместе с пользователем к другому клиенту")]
		public void Move_address_with_user_to_another_client()
		{
			var newClient = DataMother.CreateTestClientWithAddressAndUser();
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var address = oldClient.Addresses[0];
			var user = oldClient.Users[0];

			Open(address);
			// Даем доступ пользователю к адресу доставки
			browser.CheckBox(Find.ByName("delivery.AvaliableForUsers[0].Id")).Checked = true;
			browser.Button(Find.ByValue("Сохранить")).Click();
			browser.Refresh();

			browser.Link(Find.ByText(address.Value)).Click();
			// Ищем клиента, к которому нужно передвинуть пользователя и двигаем
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			//перемещение пользователя не опционально
			//Assert.That(browser.Text, Is.StringContaining(String.Format("Перемещать пользователя {0}", user.Id)));
			//Assert.IsTrue(browser.CheckBox(Find.ByName("moveWithUser")).Checked);
			browser.Button(Find.ByValue("Переместить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки успешно перемещен"));

			oldClient.Refresh();
			newClient.Refresh();
			address.Refresh();
			Assert.That(address.Client.Id, Is.EqualTo(newClient.Id));

			//перемещение пользователя не опционально
			//Assert.That(newClient.Users.Count, Is.EqualTo(2));
			//Assert.That(oldClient.Users.Count, Is.EqualTo(0));

			Assert.That(newClient.Addresses.Count, Is.EqualTo(2));
			Assert.That(oldClient.Addresses.Count, Is.EqualTo(0));
		}

		[Test, NUnit.Framework.Description("Перемещение только адреса доставки (без пользователя) к другому клиенту")]
		public void Move_only_address_to_another_client()
		{
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var newClient = DataMother.CreateTestClientWithAddressAndUser();
			var address = oldClient.Addresses[0];
			var addressIdForMove = address.Id;
			scope.Flush();

			Open(address);
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			Assert.IsTrue(browser.SelectList(Find.ById("clientsList")).Exists);
			Assert.That(browser.SelectList(Find.ById("clientsList")).Options.Count, Is.GreaterThan(0));

			Assert.IsTrue(browser.Button(Find.ByValue("Отмена")).Exists);
			Assert.IsTrue(browser.Button(Find.ByValue("Переместить")).Exists);

			browser.Button(Find.ByValue("Переместить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки успешно перемещен"));
			Assert.That(browser.Text, Is.StringContaining(newClient.Name));
			Assert.That(browser.Text, Is.Not.StringContaining(oldClient.Name));

			oldClient.Refresh();
			newClient.Refresh();
			address = Address.Find(addressIdForMove);
			Assert.That(address.Client.Id, Is.EqualTo(newClient.Id));
			Assert.That(newClient.Addresses.Count, Is.EqualTo(2));
			Assert.That(oldClient.Addresses.Count, Is.EqualTo(0));
		}

		[Test, NUnit.Framework.Description("После перемещения адреса доставки, для этого адреса должны быть скопированы записи в таблице AddressesIntersection")]
		public void After_moving_address_must_be_copied_address_intersection_entries()
		{
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var newClient = DataMother.CreateTestClientWithAddressAndUser();
			var user = oldClient.Users[0];
			var address = oldClient.Addresses[0];
			var oldCountAddressIntersectionEntries = address.GetAddressIntersectionCount();

			Open(oldClient.Addresses[0]);
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			browser.Button(Find.ByValue("Переместить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки успешно перемещен"));

			var newCountAddressInterSectionEntries = address.GetAddressIntersectionCount();

			Assert.That(oldCountAddressIntersectionEntries, Is.EqualTo(newCountAddressInterSectionEntries));
			Assert.That(GetCountAddressIntersectionEntriesForClient(address.Id, oldClient.Id), Is.EqualTo(0));
			Assert.That(GetCountAddressIntersectionEntriesForClient(address.Id, newClient.Id), Is.EqualTo(oldCountAddressIntersectionEntries));
		}

		private uint GetCountAddressIntersectionEntriesForClient(uint addressId, uint clientId)
		{
			return Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT COUNT(*)
FROM Future.AddressIntersection AS ai
	JOIN Future.Intersection AS i ON i.ClientId = :ClientId AND ai.IntersectionId = i.Id
WHERE AddressId = :AddressId
")
					.SetParameter("AddressId", addressId)
					.SetParameter("ClientId", clientId)
					.UniqueResult()));
		}
	}
}
