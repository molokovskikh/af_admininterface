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
		public void Setup_address_contact_information()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Новый адрес доставки")).Click();
				Assert.That(browser.Text, Is.StringContaining("Контактная информация"));
				browser.Button(Find.ByValue("Добавить")).Click();
				var rowId = 0;
				browser.TextField(String.Format("contacts[{0}].ContactText", --rowId)).TypeText("test@test");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Некорректный адрес электронной почты"));
				browser.TextField(String.Format("contacts[{0}].ContactText", rowId)).TypeText("test@test.ru");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Это поле необходимо заполнить"));
				browser.TextField(Find.ByName("delivery.value")).TypeText("Test address");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Test address"));
				browser.Link(Find.ByText("Test address")).Click();
				browser.Button(Find.ByValue("Добавить")).Click();
				var comboBox = browser.SelectList(Find.ByName(String.Format("contactTypes[{0}]", --rowId)));
				comboBox = browser.SelectLists[1];
				comboBox.SelectByValue(comboBox.Options[1].Value);
				browser.TextField(Find.ById(String.Format("contacts[{0}].ContactText", ++rowId))).TypeText("556677");
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Некорректный телефонный номер"));
				browser.TextField(Find.ById(String.Format("contacts[{0}].ContactText", rowId))).TypeText("123-556677");
				browser.Button(Find.ByValue("Добавить")).Click();
				browser.TextField(String.Format("contacts[{0}].ContactText", --rowId)).TypeText("test2@test.ru");
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}

			// Проверка, что контактные записи создались в БД
			IList contactIds;
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.NotNull(client.Addresses[0].ContactGroup);
				var group = client.Addresses[0].ContactGroup;
				Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id), 
					"Не совпадают Id владельца группы у клиента и у новой группы");
				var contactGroupCount = ArHelper.WithSession(s =>
                    s.CreateSQLQuery("select count(*) from contacts.contact_groups where Id = :ContactGroupId")
						.SetParameter("ContactGroupId", group.Id)
                        .UniqueResult());
				Assert.That(Convert.ToInt32(contactGroupCount), Is.EqualTo(1));
				contactIds = ArHelper.WithSession(s =>
                    s.CreateSQLQuery("select Id from contacts.contacts where ContactOwnerId = :ownerId")
						.SetParameter("ownerId", group.Id)
                        .List());
				Assert.That(contactIds.Count, Is.EqualTo(3));


			}

			// Удаление контактной записи
			using (var browser = Open("client/{0}", client.Id))
			{				
				browser.Link(Find.ByText("Test address")).Click();
				browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", contactIds[0]))).Click();
				browser.Button(Find.ByValue("Сохранить")).Click();
			}

			// Проверка, что контактная запись удалена
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				var group = client.Addresses[0].ContactGroup;
				contactIds = ArHelper.WithSession(s =>
                    s.CreateSQLQuery("select * from contacts.contacts where ContactOwnerId = :ownerId")
						.SetParameter("ownerId", group.Id)
                        .List());
				Assert.That(contactIds.Count, Is.EqualTo(2));
			}
		}
	}
}
