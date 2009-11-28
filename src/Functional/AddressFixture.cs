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
	}
}
