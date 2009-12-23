using System.Linq;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

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
				Assert.That(browser.Text, Is.StringContaining("Email"));
				Assert.That(browser.Text, Is.StringContaining("Телефон"));
				
				browser.TextField(Find.ByName("contactEmailText")).TypeText("12@12.12");				
				browser.TextField(Find.ByName("delivery.value")).TypeText("Test address");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пожалуйста, введите корректный адрес электронной почты."));
				browser.TextField(Find.ByName("contactEmailText")).TypeText("ww@ww.ru");
				browser.TextField(Find.ByName("contactPhoneText")).TypeText("ww@ww.ru");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Неправильный формат телефонного номера"));
				browser.TextField(Find.ByName("contactPhoneText")).TypeText("123-456789");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Адрес доставки создан"));

				Assert.That(browser.Text, Is.StringContaining("Test address"));
				browser.Link(Find.ByText("Test address")).Click();
				Assert.That(browser.TextField(Find.ByName("contactEmailText")).Text, Is.StringContaining("ww@ww.ru"));
				Assert.That(browser.TextField(Find.ByName("contactPhoneText")).Text, Is.StringContaining("123-456789"));
				browser.TextField(Find.ByName("delivery.value")).TypeText("Changed address");
				browser.TextField(Find.ByName("contactEmailText")).TypeText("test@test.ru");
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Changed address"));
				browser.Link(Find.ByText("Changed address")).Click();
				Assert.That(browser.TextField(Find.ByName("contactEmailText")).Text, Is.StringContaining("test@test.ru"));
			}

			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.NotNull(client.Addresses[0].ContactGroup);
				var group = client.Addresses[0].ContactGroup;
				var contacts = ArHelper.WithSession(s => 
					s.CreateSQLQuery("select * from contacts.contacts where ContactOwnerId = :ownerId")
					.SetParameter("ownerId", group.Id)
					.List());
				Assert.That(contacts.Count, Is.EqualTo(2));
			}
		}
	}
}
