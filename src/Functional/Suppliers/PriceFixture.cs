using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;

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
	}
}