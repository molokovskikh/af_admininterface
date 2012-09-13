using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;

namespace Functional.Suppliers
{
	[TestFixture]
	public class PriceFixture : WatinFixture2
	{
		private User user;
		private Supplier supplier;
		private Payer payer;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			supplier = (Supplier)user.RootService;
			payer = DataMother.CreatePayer();
			payer.Save();
		}

		[Test]
		public void Delete_cost_column()
		{
			var price = supplier.Prices[0];
			price.CostType = 1;
			var cost = price.AddCost();
			cost.CostFormRule.FieldName = "F10";
			session.Save(supplier);

			Open(supplier);
			Click("Настройка");
			Click("Базовый");
			AssertText("Настройка ценовых колонок");
			Click("Удалить");
			AssertText("Внимание! Ценовая колонка настроена");
			Click("Все равно удалить");
			AssertText("Настройка ценовых колонок");

			session.Clear();
			price = session.Load<Price>(price.Id);
			Assert.That(price.Costs.Count, Is.EqualTo(1));
		}

		[Test]
		public void Delete_cost_column_and_set_regional_base_cost()
		{
			// создаем нового поставщика
			var supplier = DataMother.CreateSupplier();
			// назначаем регионы Белгород и Воронеж
			supplier.RegionMask = 1 | 2;
			supplier.Prices[0].Costs[0].Name = "Базовая";
			supplier.Prices[0].CostType = 0;
			Save(supplier);
			Flush();
			// добавляем новые ценовые колонки к прайсу
			var price = supplier.Prices[0];
			price.Costs.Add(new Cost {
				Price = price,
				PriceItem = price.Costs[0].PriceItem,
				Name = "Новая базовая"
			});
			price.Costs.Add(new Cost {
				Price = price,
				PriceItem = price.Costs[0].PriceItem,
				Name = "Новая"
			});
			price.Costs.Add(new Cost {
				Price = price,
				PriceItem = price.Costs[0].PriceItem,
				Name = "Новая1"
			});
			price.Costs[1].CostFormRule = new CostFormRule { Cost = price.Costs[1], FieldName = "" };
			price.Costs[2].CostFormRule = new CostFormRule { Cost = price.Costs[2], FieldName = "" };
			price.Costs[3].CostFormRule = new CostFormRule { Cost = price.Costs[3], FieldName = "" };
			Save(price);
			Flush();
			// добавляем новые региональные настройки и выставляем там базовой ценой только что добавленную
			var regionalData = new PriceRegionalData {
				Enabled = true,
				Price = price,
				Region = session.Query<Region>().First(t => t.Id == 2),
				Cost = price.Costs[1]
			};
			price.RegionalData.Add(regionalData);
			Save(regionalData);
			Flush();

			// создаем нового клиента в регионе Белгород
			var client = DataMother.TestClient((c) => {
				c.HomeRegion = regionalData.Region;
				c.MaskRegion = regionalData.Region.Id;
				c.Settings.WorkRegionMask = regionalData.Region.Id;
				c.Settings.OrderRegionMask = regionalData.Region.Id;
			});
			// и еще одного в Воронеже
			var client1 = DataMother.TestClient();
			Save(client1);
			Save(client);
			Flush();
			// проверяем, что ему добавилась Белгородская базовая цена
			var intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id && i.Client == client);
			foreach (var intersectionItem in intersection) {
				Assert.That(intersectionItem.Cost == price.Costs[1]);
			}
			intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id && i.Client == client1);
			foreach (var intersectionItem in intersection) {
				Assert.That(intersectionItem.Cost == price.Costs[0]);
			}
			// выставляем базовой колонкой для Белгорода другую
			regionalData.Cost = price.Costs[2];
			// меняем базовую колонку для прайса
			price.Costs[0].BaseCost = false;
			price.Costs[3].BaseCost = true;
			Save(regionalData);
			Flush();
			// Удаляем ценовые колонки, назначенные клиенту
			Open(supplier);
			Click("Настройка");
			Click("Базовый");
			AssertText("Настройка ценовых колонок");
			Click("Удалить");
			Click("Удалить");
			AssertText("Настройка ценовых колонок");
			// проверяем, что клиенту назначилась верная базовая колонка
			intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id
				&& i.Client == client
				&& i.Cost.Id == price.Costs[2].Id);
			Assert.That(intersection.Count() == 1);
			intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id
				&& i.Client == client1
				&& i.Cost.Id == price.Costs[3].Id);
			Assert.That(intersection.Count() == 1);
		}

		[Test]
		public void Edit_price_name()
		{
			Open(supplier);
			Click("Настройка");
			Click("Редактировать");
			AssertText("Редактирование прайса Базовый");
			Css("#price_Name").TypeText("Тестовый");
			Click("Сохранить");
			AssertText("Сохранено");

			session.Clear();
			var price = session.Load<Price>(supplier.Prices[0].Id);
			Assert.That(price.Name, Is.EqualTo("Тестовый"));
		}

		[Test]
		public void RegionalBaseCostTest()
		{
			supplier.Prices[0].Costs.Add(new Cost {
				Name = "Новая базовая",
				Price = supplier.Prices[0]
			});
			supplier.Prices[0].CostType = 0;
			Save(supplier);
			Flush();
			Open(supplier);
			Click("Настройка");
			Click("Базовый");
			var selectList = browser.SelectLists[0];
			selectList.Select("Новая базовая");
			Click("Применить");
			AssertText("Сохранено");
			Click("Настройка поставщика");
			Click("Базовый");
			selectList = browser.SelectLists[0];
			Assert.That(selectList.SelectedItem, Is.EqualTo("Новая базовая"));
		}
	}
}