using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using Test.Support;
using NUnit.Framework;


namespace Integration
{
	public class RegionalBaseCostsFixture : IntegrationFixture
	{
		[Test]
		public void SetRegionalCostToIntersectionTest()
		{
			// создаем нового поставщика
			var supplier = DataMother.CreateSupplier();
			// назначаем регионы Белгород и Воронеж
			supplier.RegionMask = 1 | 2;
			supplier.Prices[0].Costs[0].Name = "Базовая";
			Save(supplier);
			Flush();
			// добавляем новую ценовую колонку к прайсу
			var price = supplier.Prices[0];
			price.Costs.Add(new Cost {
				Price = price,
				PriceItem = price.Costs[0].PriceItem,
				Name = "Новая"
			});
			price.Costs[1].CostFormRule = new CostFormRule { Cost = price.Costs[1], FieldName = "" };
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
			// вставка в intersection
			Maintainer.MaintainIntersection(supplier);
			// проверяем, что все вставилось с правильной базовой ценой
			var intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id);
			foreach (var intersectionItem in intersection) {
				if (intersectionItem.Region == regionalData.Region) {
					Assert.That(intersectionItem.Cost == price.Costs[1]);
				}
				else {
					Assert.That(intersectionItem.Cost == price.Costs[0]);
				}
			}
			// создаем нового клиента в регионе Белгород
			var client = DataMother.TestClient((c) => {
				c.HomeRegion = regionalData.Region;
				c.MaskRegion = regionalData.Region.Id;
				c.Settings.WorkRegionMask = regionalData.Region.Id;
				c.Settings.OrderRegionMask = regionalData.Region.Id;
			});
			Save(client);
			Flush();
			// проверяем, что ему добавилась Белгородская базовая цена
			intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id && i.Client == client);
			foreach (var intersectionItem in intersection) {
				Assert.That(intersectionItem.Cost == price.Costs[1]);
			}
			// создаем клиента в Воронеже
			client = DataMother.TestClient();
			Save(client);
			Flush();
			// проверяем, что у него установлена Воронежская базовая цена
			intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id && i.Client == client);
			foreach (var intersectionItem in intersection) {
				Assert.That(intersectionItem.Cost == price.Costs[0]);
			}
		}
	}
}
