using System;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using log4net.Config;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using Common.Web.Ui.Models;
using System.Threading;

namespace Functional.Drugstore
{
	public class AddressFixture : FunctionalFixture
	{
		private Client client;

		[SetUp]
		public void Setup()
		{
			var supplier = DataMother.CreateSupplier();
			Save(supplier);
			client = DataMother.CreateTestClientWithUser();
			Flush();
			Open(client);
			AssertText("Клиент");
		}

		[Test]
		public void Memo_about_writing_addresses_test()
		{
			var defaultSettings = session.Query<DefaultValues>().First();
			defaultSettings.AddressesHelpText = "Тестовый текст памятки адреса";
			session.Save(defaultSettings);
			Flush();
			Click("Новый пользователь");
			AssertText("Тестовый текст памятки адреса");
			Open(client);
			Click("Новый адрес доставки");
			AssertText("Тестовый текст памятки адреса");
			Css("#address_Value").AppendText("Тестовые адрес доставки");
			Click("Создать");
			Open(client);
			Click("Тестовые адрес доставки");
			AssertText("Тестовый текст памятки адреса");
		}

		[Test]
		public void DeleteContactInformation()
		{
			// Удаление контактной записи
			var countContacts = AddContactsToNewDeliveryAddress(client.Id);
			session.Refresh(client);
			var group = client.Addresses[0].ContactGroup;
			browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
			Thread.Sleep(500);
			ClickButton("Сохранить");

			// Проверка, что контактная запись удалена
			Thread.Sleep(500);
			var count = ContactInformationHelper.GetCountContactsInDb(session, client.Addresses[0].ContactGroup);
			Assert.That(count, Is.EqualTo(countContacts - 1));
		}

		[Test]
		public void Create_address()
		{
			ClickLink("Новый адрес доставки");
			AssertText("Новый адрес доставки");
			browser.TextField(Find.ByName("address.Value")).TypeText("тестовый адрес");
			ClickButton("Создать");
			AssertText("Адрес доставки создан");

			session.Refresh(client);
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
			AssertText("Адрес доставки");
			ClickButton("Отправить уведомления о регистрации поставщикам");
			AssertText("Уведомления отправлены");
		}

		// Создает новый адрес доставки и 3 контакта для него (2 email)
		private int AddContactsToNewDeliveryAddress(uint clientId)
		{
			var applyButtonText = "Создать";
			ClickLink("Новый адрес доставки");
			browser.TextField(Find.ByName("address.Value")).TypeText("Test address");
			ContactInformationHelper.AddContact(browser, ContactType.Email, applyButtonText, clientId);
			AssertText("Адрес доставки создан");
			ClickLink("Test address");
			applyButtonText = "Сохранить";
			ContactInformationHelper.AddContact(browser, ContactType.Email, applyButtonText, clientId);
			AssertText("Сохранено");
			ClickLink("Test address");
			ContactInformationHelper.AddContact(browser, ContactType.Phone, applyButtonText, clientId);
			AssertText("Сохранено");
			ClickLink("Test address");
			return 3;
		}

		[Test]
		public void AddContactInformation()
		{
			var countContacts = AddContactsToNewDeliveryAddress(client.Id);
			// Проверка, что контактные записи создались в БД

			session.Refresh(client);
			Assert.NotNull(client.Addresses[0].ContactGroup);
			var group = client.Addresses[0].ContactGroup;
			Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
				"Не совпадают Id владельца группы у клиента и у новой группы");

			ContactInformationHelper.CheckContactGroupInDb(session, group);
			countContacts = ContactInformationHelper.GetCountContactsInDb(session, group);
			Assert.That(countContacts, Is.EqualTo(countContacts));
		}

		[Test]
		public void Address_must_be_enabled_after_registration()
		{
			ClickLink("Новый адрес доставки");
			browser.TextField(Find.ByName("address.Value")).TypeText("test address");
			ClickButton("Создать");
			AssertText("Адрес доставки создан");

			session.Refresh(client);
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
			browser.CheckBox(Find.ByName("address.AvaliableForUsers[0].Id")).Checked = true;
			ClickButton("Сохранить");
			Refresh();

			ClickLink(address.Value);
			// Ищем клиента, к которому нужно передвинуть пользователя и двигаем
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			//перемещение пользователя не опционально
			//AssertText(String.Format("Перемещать пользователя {0}", user.Id));
			//Assert.IsTrue(browser.CheckBox(Find.ByName("moveWithUser")).Checked);
			ClickButton("Переместить");
			AssertText("Адрес доставки успешно перемещен");

			session.Refresh(oldClient);
			session.Refresh(newClient);
			session.Refresh(address);
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
			Flush();

			Open(address);
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			Assert.IsTrue(browser.SelectList(Find.ById("clientsList")).Exists);
			Assert.That(browser.SelectList(Find.ById("clientsList")).Options.Count, Is.GreaterThan(0));

			Assert.IsTrue(browser.Button(Find.ByValue("Отмена")).Exists);
			Assert.IsTrue(browser.Button(Find.ByValue("Переместить")).Exists);

			ClickButton("Переместить");
			AssertText("Адрес доставки успешно перемещен");
			AssertText(newClient.Name);
			Assert.That(browser.Text, Is.Not.StringContaining(oldClient.Name));

			session.Refresh(oldClient);
			session.Refresh(newClient);
			address = session.Load<Address>(addressIdForMove);
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
			ClickButton("Переместить");
			AssertText("Адрес доставки успешно перемещен");

			var newCountAddressInterSectionEntries = address.GetAddressIntersectionCount();

			Assert.That(oldCountAddressIntersectionEntries, Is.EqualTo(newCountAddressInterSectionEntries));
			Assert.That(GetCountAddressIntersectionEntriesForClient(address.Id, oldClient.Id), Is.EqualTo(0));
			Assert.That(GetCountAddressIntersectionEntriesForClient(address.Id, newClient.Id), Is.EqualTo(oldCountAddressIntersectionEntries));
		}

		private uint GetCountAddressIntersectionEntriesForClient(uint addressId, uint clientId)
		{
			return Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT COUNT(*)
FROM Customers.AddressIntersection AS ai
	JOIN Customers.Intersection AS i ON i.ClientId = :ClientId AND ai.IntersectionId = i.Id
WHERE AddressId = :AddressId
")
				.SetParameter("AddressId", addressId)
				.SetParameter("ClientId", clientId)
				.UniqueResult()));
		}

		[Test]
		public void AddMaxOrdersSumTest()
		{
			var address = client.AddAddress("Тестовый адрес");
			address.MaxDailyOrdersSum = 0;
			session.Save(address);
			Open(address);
			AssertText("Проверять максимальный дневной заказ");
			AssertText("Максимальная сумма заказа за 1 день, руб.:");
			Assert.That(browser.CheckBox("address_CheckDailyOrdersSum").Checked, Is.False);
			browser.CheckBox("address_CheckDailyOrdersSum").Checked = true;
			Css("#address_MaxDailyOrdersSum").Value = "100";
			Click("Сохранить");
			AssertText("Сохранено");
			Open(address);
			Assert.That(browser.CheckBox("address_CheckDailyOrdersSum").Checked, Is.True);
			Assert.That(Css("#address_MaxDailyOrdersSum").Value, Is.EqualTo("100"));
		}

		[Test]
		public void CreateAddressWithMaxOrdersSum()
		{
			ClickLink("Новый адрес доставки");
			AssertText("Новый адрес доставки");
			browser.CheckBox("address_CheckDailyOrdersSum").Checked = true;
			Css("#address_MaxDailyOrdersSum").Value = "100";
			browser.TextField(Find.ByName("address.Value")).TypeText("тестовый адрес");
			ClickButton("Создать");
			AssertText("Адрес доставки создан");

			session.Refresh(client);
			Assert.That(client.Addresses.Count, Is.EqualTo(1), "не создали адресс доставки");
			var address = client.Addresses.First();
			Assert.That(address.Value, Is.EqualTo("тестовый адрес"));
			Assert.That(address.MaxDailyOrdersSum, Is.EqualTo(100));
			Assert.That(address.CheckDailyOrdersSum, Is.True);
		}
	}
}