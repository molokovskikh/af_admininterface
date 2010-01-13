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

namespace Functional
{
	public class AddressFixture : WatinFixture
	{
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
		private int AddContactsToNewDeliveryAddress(IE browser)
		{
			var applyButtonText = "Создать";
			browser.Link(Find.ByText("Новый адрес доставки")).Click();
			browser.TextField(Find.ByName("delivery.value")).TypeText("Test address");
			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText);
			Assert.That(browser.Text, Is.StringContaining("Адрес доставки создан"));
			browser.Link(Find.ByText("Test address")).Click();
			applyButtonText = "Сохранить";
			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			browser.Link(Find.ByText("Test address")).Click();
			ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText);
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
				countContacts = AddContactsToNewDeliveryAddress(browser);
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
		public void DeleteContactInformation()
		{
			var client = DataMother.CreateTestClientWithUser();
			var countContacts = 0;
			// Удаление контактной записи
			using (var browser = Open("client/{0}", client.Id))
			{
				countContacts = AddContactsToNewDeliveryAddress(browser);				
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Addresses[0].ContactGroup;
					browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
					browser.Button(Find.ByValue("Сохранить")).Click();
				}
			}
			// Проверка, что контактная запись удалена
			var count = ContactInformationFixture.GetCountContactsInDb(client.Addresses[0].ContactGroup);
			Assert.That(count, Is.EqualTo(countContacts - 1));
		}
	}
}
