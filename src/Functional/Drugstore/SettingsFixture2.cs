using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using Test.Support.Selenium;

namespace Functional.Drugstore
{
	public class SettingsFixture2 : AdmSeleniumFixture
	{
		private Client client;
		private DrugstoreSettings settings;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			Flush();
			settings = client.Settings;

			Open(client, "Settings");
			AssertText("Конфигурация клиента");
		}

		[Test]
		public void Show_smart_order_rule_parser_configuration()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.Name = "Поставщик для тестирования";
				s.FullName = "Поставщик для тестирования";
				s.AddPrice("Ассортиментный прайс", PriceType.Assortment);
			});
			Save(supplier);

			//отключаем
			Css("#drugstore_EnableSmartOrder").Click();
			//включаем
			Css("#drugstore_EnableSmartOrder").Click();
			SearchV2("#drugstore_SmartOrderRules_ParseAlgorithm", "DbfSource");
			Assert.IsTrue(Css("#drugstore_SmartOrderRules_CodeColumn").Displayed);
			Assert.IsFalse(Css("#drugstore_SmartOrderRules_StartLine").Displayed);
			Click("Сохранить");
			Assert.AreEqual(Error("#drugstore_SmartOrderRules_QuantityColumn"), "Это поле необходимо заполнить.");
			Assert.AreEqual(Error("#drugstore_SmartOrderRules_ProductColumn"), "Это поле необходимо заполнить.");
			SearchV2("#drugstore_SmartOrderRules_AssortimentPriceCode_Id", "Поставщик для тестирования");
			Css("#drugstore_SmartOrderRules_ProductColumn").SendKeys("F1");
			Css("#drugstore_SmartOrderRules_QuantityColumn").SendKeys("F2");
			Click("Сохранить");
			AssertText("Сохранено");

			session.Refresh(settings.SmartOrderRules);
			Assert.AreEqual(settings.SmartOrderRules.ProductColumn, "F1");
		}

		private string Error(string selector)
		{
			var label = browser.FindElementByCssSelector(selector).FindElement(By.XPath("..")).FindElement(By.CssSelector("label.error"));
			if (label == null)
				return "";
			return label.Text;
		}

		protected object SearchV2(string css, string term)
		{
			var root = browser.FindElementByCssSelector(css).FindElement(By.XPath(".."));
			var searchInput = root.FindElements(By.CssSelector(".term")).FirstOrDefault();
			if (searchInput == null) {
				var button = root.FindElement(By.CssSelector("[type=button]"));
				ScrollTo(button);
				button.Click();
				WaitForCss(".term", root);
				searchInput = root.FindElement(By.CssSelector(".term"));
			}
			searchInput.SendKeys(term);
			ClickButton("Найти", root);
			WaitAjax();
			return root.FindElement(By.CssSelector("select"));
		}
	}
}