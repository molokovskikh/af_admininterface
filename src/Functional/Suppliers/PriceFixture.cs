using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using Functional.Drugstore;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;

namespace Functional.Suppliers
{
	[TestFixture]
	public class PriceFixture : FunctionalFixture
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

		[Test(Description = "Тестирует назначение базовой ценовой колонки по-умолчанию для региональной настройки")]
		public void AddPriceRegionalDataWithBaseCost()
		{
			var price = supplier.Prices[0];
			price.Costs[0].Name = "Базовая";
			price.CostType = 0;
			var regionalData = price.RegionalData.First();
			regionalData.Cost = price.Costs[0];
			Save(regionalData);
			Flush();
			Open(supplier);
			Click("Настройка");
			browser.ShowWindow(NativeMethods.WindowShowStyle.Maximize);
			browser.CheckBox("MainContentPlaceHolder_WorkRegionList_0").Checked = true;
			Click("Применить");
			browser.ShowWindow(NativeMethods.WindowShowStyle.Maximize);
			session.Clear();
			var savedPrice = session.Load<Price>(price.Id);
			Assert.That(savedPrice.RegionalData.Count(d => d.Cost.Id == regionalData.Cost.Id), Is.EqualTo(2));
		}

		[Test]
		public void Delete_cost_column_and_set_regional_base_cost()
		{
			// создаем нового поставщика
			var supplier = DataMother.CreateSupplier();
			supplier.Prices[0].Costs[0].Name = "Базовая";
			supplier.Prices[0].CostType = 0;
			Save(supplier);
			// добавляем новые ценовые колонки к прайсу
			var price = supplier.Prices[0];
			price.Costs.Add(new Cost {
				Price = price,
				PriceItem = price.Costs[0].PriceItem,
				Name = "Новая базовая"
			});

			price.Costs[1].CostFormRule = new CostFormRule { Cost = price.Costs[1], FieldName = "" };
			Save(price);
			// добавляем новые региональные настройки и выставляем там базовой ценой только что добавленную
			var regionalData = price.RegionalData.First();
			regionalData.Cost = price.Costs[1];
			Save(regionalData);

			// создаем нового клиента
			var client = DataMother.TestClient();

			// проверяем, что ему добавилась базовая цена
			var intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id && i.Client == client);
			foreach (var intersectionItem in intersection)
				Assert.That(intersectionItem.Cost == price.Costs[1]);

			// выставляем базовой колонкой другую
			regionalData.Cost = price.Costs[0];
			Save(regionalData);
			Flush();
			// Удаляем ценовые колонки, назначенные клиенту
			Open(supplier);
			Click("Настройка");
			Click("Базовый");
			AssertText("Настройка ценовых колонок");
			Click("Удалить");
			// проверяем, что клиенту назначилась верная базовая колонка
			intersection = session.Query<Intersection>().Where(i => i.Price.Id == price.Id
				&& i.Client == client
				&& i.Cost.Id == price.Costs[0].Id);
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
				Price = supplier.Prices[0],
				PriceItem = supplier.Prices[0].Costs[0].PriceItem
			});
			supplier.Prices[0].Costs[1].CostFormRule = new CostFormRule {
				Cost = supplier.Prices[0].Costs[1], FieldName = ""
			};
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
			browser.ShowWindow(NativeMethods.WindowShowStyle.Maximize);
			Click("Базовый");
			selectList = browser.SelectLists[0];
			Assert.That(selectList.SelectedItem, Is.EqualTo("Новая базовая"));
		}

		[Test]
		public void IsLocalPriceTest()
		{
			Open(supplier);
			Click("Настройка");
			AssertText("Локальный");
			var localBox = browser.CheckBox(Find.ById("MainContentPlaceHolder_PricesGrid_IsLocal_0"));
			localBox.Checked = true;
			Click("Применить");
			Assert.That(browser.CheckBox(Find.ById("MainContentPlaceHolder_PricesGrid_IsLocal_0")).Checked, Is.True);
		}

		[Test]
		public void Set_price_as_join_matrix()
		{
			var matrixSupplier = dataMother.CreateMatrix();
			var matrixPrice = matrixSupplier.Prices[1];

			var price = supplier.Prices.First();
			Open(price, "Edit");
			Css("#price_IsMatrix").Click();

			Search(matrixSupplier.Name, "Объединить с матрицей прайс листа");
			var select = SearchRoot("Объединить с матрицей прайс листа").Css("select");
			select.SelectByValue(matrixPrice.Id.ToString());
			browser.Eval("$('select').change()");
			Assert.That(select.SelectedItem, Is.EqualTo(matrixPrice.ToString()));
			Click("Сохранить");
			AssertText("Сохранено");
			AssertText(matrixPrice.ToString());

			session.Refresh(price);
			Assert.That(price.Matrix.Id, Is.EqualTo(matrixPrice.Matrix.Id));
		}
	}
}