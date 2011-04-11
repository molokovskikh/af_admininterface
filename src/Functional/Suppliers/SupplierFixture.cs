using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;
using WatiN.Core;
using WatiNCssSelectorExtensions;

namespace Functional.Suppliers
{
	public class SupplierFixture : WatinFixture2
	{
		private User user;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			scope.Flush();
		}

		[Test]
		public void Search_supplier_user()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText(user.Id.ToString());
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.Text, Is.StringContaining(user.Login));

			browser.Link(Find.ByText(user.Id.ToString())).Click();
			Assert.That(browser.Text, Is.StringContaining("Поставщик"));
		}

		[Test]
		public void Search_supplier()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText("Тестовый поставщик");
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.Text, Is.StringContaining("Тестовый поставщик"));

			browser.Link(Find.ByText("Тестовый поставщик")).Click();
			Assert.That(browser.Text, Is.StringContaining("Поставщик"));
		}
	}
}