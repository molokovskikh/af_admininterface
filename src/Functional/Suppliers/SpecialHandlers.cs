using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;

namespace Functional.Suppliers
{
	public class SpecialHandlers : FunctionalFixture
	{
		private User user;
		private Supplier supplier;

		[SetUp]
		public void Setup()
		{
			user = DataMother.CreateSupplierUser();
			supplier = (Supplier)user.RootService;
		}

		[Test]
		public void Show_handlers()
		{
			Open(supplier);
			Click("Настройка");
			AssertText("Конфигурация клиента");
			Click("Настройка форматеров и отправщиков доступных поставщику");
			AssertText("Обработчики заказов");
		}
	}
}