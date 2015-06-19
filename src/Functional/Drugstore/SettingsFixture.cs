using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using WatiN.Core.Native.Windows;
using WatiN.CssSelectorExtensions;
using DescriptionAttribute = NUnit.Framework.DescriptionAttribute;

namespace Functional.Drugstore
{
	[TestFixture]
	public class SettingsFixture : FunctionalFixture
	{
		private Client client;
		private DrugstoreSettings settings;

		[SetUp]
		public new void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			Flush();
			settings = client.Settings;

			Open(client, "Settings");
			AssertText("Конфигурация клиента");
		}

		[Test]
		public void Set_buying_matrix_configuration()
		{
			DataMother.CreateMatrix();

			Css("#drugstore_EnableBuyingMatrix").Click();

			var select = Search("Фармаимпекс", "Ассортиментный прайс для матрицы предложений");
			Assert.That(select.SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.BuyingMatrixType")).SelectedItem, Is.EqualTo("Белый список"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.BuyingMatrixAction")).SelectedItem, Is.EqualTo("Запретить заказ"));

			ClickButton("Сохранить");
			AssertText("Сохранено");
			browser.Click("Настройка");

			AssertText("Фармаимпекс - Матрица");
			Assert.That(browser.SelectList(Find.ByName("drugstore.BuyingMatrixType")).SelectedItem, Is.EqualTo("Белый список"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.BuyingMatrixAction")).SelectedItem, Is.EqualTo("Запретить заказ"));

			session.Refresh(client);

			Assert.That(client.Settings.BuyingMatrixPrice.Name, Is.EqualTo("Матрица"));
			Assert.That(client.Settings.BuyingMatrixType, Is.EqualTo(MatrixType.WhiteList));
			Assert.That(client.Settings.BuyingMatrixAction, Is.EqualTo(MatrixAction.Block));
		}

		[Test]
		public void Set_offers_matrix()
		{
			DataMother.CreateMatrix();

			Css("#drugstore_EnableOfferMatrix").Click();
			var select = Search("Фармаимпекс", "Ассортиментный прайс для матрицы закупок");
			Assert.That(select.SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.OfferMatrixType")).SelectedItem, Is.EqualTo("Белый список"));

			ClickButton("Сохранить");
			AssertText("Сохранено");
			browser.Click("Настройка");

			AssertText("Фармаимпекс - Матрица");
			Assert.That(browser.SelectList(Find.ByName("drugstore.OfferMatrixType")).SelectedItem, Is.EqualTo("Белый список"));

			session.Refresh(client);

			Assert.That(client.Settings.OfferMatrixPrice.Name, Is.EqualTo("Матрица"));
			Assert.That(client.Settings.OfferMatrixType, Is.EqualTo(MatrixType.WhiteList));
		}

		[Test]
		public void Add_offer_matrix_exclude()
		{
			var supplier = DataMother.CreateMatrix();
			Maintainer.MaintainIntersection(session, client, client.Orgs().First());

			Css("#drugstore_EnableOfferMatrix").Click();
			var select = Search("Фармаимпекс", "Ассортиментный прайс для матрицы закупок");
			Assert.That(select.SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));

			var excludes = ((Table)Css("#excludes"));
			excludes.Css("input[value='Добавить']").Click();
			excludes.Css(".term").TypeText("Фармаимпекс");
			excludes.Css(".search[type=button]").Click();
			Thread.Sleep(1000);
			Assert.That(excludes.Css("div.search select").SelectedItem, Is.StringEnding("Фармаимпекс"));

			Click("Сохранить");
			AssertText("Сохранено");
			Click("Настройка");

			AssertText("Фармаимпекс - Матрица");
			Assert.That(browser.SelectList(Find.ByName("drugstore.OfferMatrixType")).SelectedItem, Is.EqualTo("Белый список"));
			excludes = ((Table)Css("#excludes"));
			Assert.That(excludes.Text, Is.StringContaining("Фармаимпекс"));

			session.Refresh(client);

			Assert.That(client.Settings.OfferMatrixPrice.Name, Is.EqualTo("Матрица"));
			Assert.That(client.Settings.OfferMatrix, Is.Not.Null);
			Assert.That(client.Settings.OfferMatrixType, Is.EqualTo(MatrixType.WhiteList));
			Assert.That(client.Settings.OfferMatrixExcludes.Count, Is.EqualTo(1));
			Assert.That(client.Settings.OfferMatrixExcludes[0].Name, Is.EqualTo(supplier.Name));
		}

		[Test]
		public void Check_default_smart_order_settings()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.Name = "Фармаимпекс";
				s.FullName = "Фармаимпекс";
				s.AddPrice("Матрица", PriceType.Assortment);
			});
			Save(supplier);
			session.Save(new ParseAlgorithm("TextSource"));
			Flush();

			var price = SearchV2(Css("#drugstore_SmartOrderRules_AssortimentPriceCode_Id"), "Фармаимпекс");
			Assert.That(price.SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));

			Click("Сохранить");
			AssertText("Сохранено");
		}

		[Test]
		public void Add_parser_and_assortiment_price()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.Name = "Фармаимпекс";
				s.FullName = "Фармаимпекс";
				s.AddPrice("Матрица", PriceType.Assortment);
			});
			Save(supplier);
			session.Save(new ParseAlgorithm("TextSource"));
			Flush();

			var parser = SearchV2(Css("#drugstore_SmartOrderRules_ParseAlgorithm"), "TextSource");
			Assert.That(parser.SelectedItem, Is.EqualTo("TextSource"));

			var price = SearchV2(Css("#drugstore_SmartOrderRules_AssortimentPriceCode_Id"), "Фармаимпекс");
			Assert.That(price.SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));

			Css("#drugstore_SmartOrderRules_ColumnSeparator").TypeText(@"\t");
			Css("#drugstore_SmartOrderRules_CodePage").Select("windows-1251");
			Css("#drugstore_SmartOrderRules_ProductColumn").TypeText("0");
			Css("#drugstore_SmartOrderRules_QuantityColumn").TypeText("1");

			Click("Сохранить");
			AssertText("Сохранено");
			Click("Настройка");
			Assert.IsTrue(Css("#drugstore_EnableSmartOrder").Checked);
			AssertText("Фармаимпекс - Матрица");
			AssertText("TextSource");

			var rule = client.Settings.SmartOrderRules;
			session.Refresh(rule);
			Assert.AreEqual(@"\t", rule.ColumnSeparator);
			Assert.AreEqual(1251, rule.CodePage);
			Assert.AreEqual("0", rule.ProductColumn);
			Assert.AreEqual("1", rule.QuantityColumn);
		}

		[Test]
		public void Try_to_send_email_notification()
		{
			Open(client, "Settings");
			ClickButton("Отправить уведомления о регистрации поставщикам");
			AssertText("Уведомления отправлены");
		}

		[Test]
		public void Try_to_change_home_region_for_drugstore()
		{
			var homeRegionSelect = GetHomeRegionSelect(browser);
			var changeTo = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
			homeRegionSelect.Select(changeTo);
			Click("Сохранить");
			AssertText("Сохранено");
			ClickLink("Настройка");
			var selectedText = GetHomeRegionSelect(browser).SelectedOption.Text;
			Assert.That(selectedText, Is.EqualTo(changeTo));
		}

		[Test]
		public void Change_work_region_and_order_region()
		{
			var workRegion = GetWorkRegion(browser, "Курск");
			Assert.That(workRegion.Checked, Is.False);
			workRegion.Checked = true;
			Assert.That(GetWorkRegion(browser, "Воронеж").Checked, Is.True);

			var orderRegion = GetOrderRegion(browser, "Липецк");
			Assert.That(orderRegion.Checked, Is.False);
			orderRegion.Checked = true;
			Assert.That(GetOrderRegion(browser, "Воронеж").Checked, Is.True);

			browser.Button(b => b.Value == "Сохранить").Click();
			Assert.That(browser.ContainsText("Сохранено"), Is.True);
			ClickLink("Настройка");

			Assert.That(GetWorkRegion(browser, "Воронеж").Checked, Is.True);
			Assert.That(GetWorkRegion(browser, "Курск").Checked, Is.True);
			Assert.That(GetWorkRegion(browser, "Липецк").Checked, Is.True);
			Assert.That(GetOrderRegion(browser, "Воронеж").Checked, Is.True);
			Assert.That(GetOrderRegion(browser, "Липецк").Checked, Is.True);
		}


		[Test, Description("При добавлении региона работы клиенту, он должен добавиться пользователю")]
		public void After_add_client_work_region_user_region_settings_must_be_update()
		{
			var workRegion = GetWorkRegion(browser, "Курск");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "Сохранить").Click();
			ClickLink(client.Users[0].Login);
			Click("Настройка");

			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));

			Assert.IsTrue(UserOrderRegionExists(browser, "Воронеж"));
			Assert.IsFalse(UserOrderRegionExists(browser, "Курск"));

			var user = client.Users[0];
			session.Refresh(user);
			Assert.IsTrue((user.WorkRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 4) > 0);
			Assert.IsTrue((user.OrderRegionMask & 1) > 0);
			Assert.IsFalse((user.OrderRegionMask & 4) > 0);
		}

		[Test, Description("При удалении региона работы у клиента, он должен удалиться у пользователя")]
		public void After_remove_client_work_region_user_region_settings_must_be_update()
		{
			var workRegion = GetWorkRegion(browser, "Курск");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "Сохранить").Click();
			ClickLink(client.Users[0].Login);
			Click("Настройка");
			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));

			browser.Back();
			browser.Back();
			ClickLink("Настройка");
			workRegion = GetWorkRegion(browser, "Воронеж");
			workRegion.Checked = false;
			browser.Button(b => b.Value == "Сохранить").Click();
			ClickLink(client.Users[0].Login);
			Click("Настройка");

			Assert.IsFalse(UserWorkRegionExists(browser, "Воронеж"));
			// При удалении региона работы должен автоматически удаляться регион заказа
			Assert.IsFalse(UserOrderRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));

			session.Refresh(client);
			var user = client.Users[0];
			session.Refresh(user);
			Assert.IsFalse((user.WorkRegionMask & 1) > 0);
			Assert.IsFalse((user.OrderRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 4) > 0);
		}

		[Test, Description("При добавлении региона заказа клиенту, он должен добавиться пользователю")]
		public void After_add_client_order_region_user_region_settings_must_be_update()
		{
			var workRegion = GetOrderRegion(browser, "Липецк");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "Сохранить").Click();
			ClickLink(client.Users[0].Login);
			Click("Настройка");
			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserOrderRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserOrderRegionExists(browser, "Липецк"));
			// При добавлении региона заказа, автоматически должен подключаться такой же регион работы
			Assert.IsTrue(UserWorkRegionExists(browser, "Липецк"));

			var user = client.Users[0];
			session.Refresh(user);
			Assert.IsTrue((user.WorkRegionMask & 1) > 0);
			Assert.IsTrue((user.OrderRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 8) > 0);
			Assert.IsTrue((user.OrderRegionMask & 8) > 0);
		}

		[Test, Description("При удалении региона заказа у клиента, он должен удалиться у пользователя")]
		public void After_remove_client_order_region_user_region_settings_must_be_update()
		{
			// Когда добавляем регион заказа, автоматически добавляется такой же регион работы
			var workRegion = GetOrderRegion(browser, "Курск");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "Сохранить").Click();
			ClickLink(client.Users[0].Login);
			Click("Настройка");

			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));
			Assert.IsTrue(UserOrderRegionExists(browser, "Курск"));

			browser.Back();
			browser.Back();
			ClickLink("Настройка");
			workRegion = GetOrderRegion(browser, "Курск");
			workRegion.Checked = false;
			browser.Button(b => b.Value == "Сохранить").Click();
			ClickLink(client.Users[0].Login);
			Click("Настройка");

			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserOrderRegionExists(browser, "Воронеж"));
			Assert.IsFalse(UserOrderRegionExists(browser, "Курск"));
			// При удалении региона заказа, регион работы должен оставаться
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));
			session.Refresh(client);
			var user = client.Users[0];
			session.Refresh(user);
			Assert.IsTrue((user.WorkRegionMask & 1) > 0);
			Assert.IsTrue((user.OrderRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 4) > 0);
			Assert.IsFalse((user.OrderRegionMask & 4) > 0);
		}

		[Test]
		public void Show_all_regions()
		{
			var countVisibleRegions = browser.Table("RegionsTable").TableRows.Count();

			ClickLink("Показать все регионы");
			var countAllRegions = browser.Table("RegionsTable").TableRows.Count();
			Assert.That(countVisibleRegions, Is.LessThan(countAllRegions));

			ClickLink("Показать только регионы по умолчанию");
			Thread.Sleep(500);
			var count = browser.Table("RegionsTable").TableRows.Count();
			Assert.That(count, Is.EqualTo(countVisibleRegions));
		}

		[Test]
		public void Regions_must_be_checked_when_show_all()
		{
			var browseCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("browseRegion"));
			var orderCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("orderRegion"));
			ClickLink("Показать все регионы");
			var allBrowseCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("browseRegion"));
			var allOrderCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("orderRegion"));
			foreach (var box in browseCheckboxes)
				foreach (var checkbox in allBrowseCheckboxes)
					if (box.Id.Equals(checkbox.Id))
						Assert.That(box.Checked, Is.EqualTo(checkbox.Checked));
			foreach (var box in orderCheckboxes)
				foreach (var checkbox in allOrderCheckboxes)
					if (box.Id.Equals(checkbox.Id))
						Assert.That(box.Checked, Is.EqualTo(checkbox.Checked));
		}

		[Test]
		public void Change_common_settings()
		{
			// "Скрывать клиента в интерфейсе поставщика"
			var checkBoxHide = Css("#drugstore_IsHiddenFromSupplier");
			Assert.That(checkBoxHide.Checked, Is.False);
			checkBoxHide.Checked = true;
			Click("Сохранить");
			AssertText("Сохранено");
			ClickLink("Настройка");
			Assert.That(Css("#drugstore_IsHiddenFromSupplier").Checked, Is.True);

			// "Сотрудник АК Инфорум"
			Css("#drugstore_ServiceClient").Checked = true;
			Click("Сохранить");
			AssertText("Сохранено");
			ClickLink("Настройка");
			Assert.That(Css("#drugstore_IsHiddenFromSupplier").Checked, Is.True);
			Assert.That(Css("#drugstore_ServiceClient").Checked, Is.True);
		}

		[Test]
		public void When_change_home_region_it_must_be_selected()
		{
			var homeRegionSelect = GetHomeRegionSelect(browser);
			var newHomeRegionName = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
			homeRegionSelect.Select((newHomeRegionName));
			Thread.Sleep(500);
			Assert.IsTrue(GetWorkRegion(browser, newHomeRegionName).Checked);
			Assert.IsTrue(GetOrderRegion(browser, newHomeRegionName).Checked);
		}

		[Test]
		public void Test_option_ignore_new_prices()
		{
			var controlName = "drugstore.IgnoreNewPrices";
			Assert.IsFalse(browser.CheckBox(Find.ByName(controlName)).Checked);
			browser.CheckBox(Find.ByName(controlName)).Checked = true;
			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Refresh(client);
			Assert.IsTrue(settings.IgnoreNewPrices);

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			ClickLink("Настройка");
			Assert.IsTrue(browser.CheckBox(Find.ByName(controlName)).Checked);
			browser.CheckBox(Find.ByName(controlName)).Checked = false;
			ClickButton("Сохранить");

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			ClickLink("Настройка");
			Assert.IsFalse(browser.CheckBox(Find.ByName(controlName)).Checked);
		}

		[Test]
		public void Max_weekly_orders_sum()
		{
			var controlName = "drugstore.MaxWeeklyOrdersSum";
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("0"));
			browser.TextField(Find.ByName(controlName)).TypeText("123456");
			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Refresh(settings);
			Assert.That(settings.MaxWeeklyOrdersSum, Is.EqualTo(123456));

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			ClickLink("Настройка");
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("123456"));
			browser.TextField(Find.ByName(controlName)).Clear();
			ClickButton("Сохранить");

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			ClickLink("Настройка");
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("0"));
		}

		[Test, Description("Зашумлять цены для всех поставщиков кроме одного")]
		public void Set_costs_noising_except_one_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			MakeNameUniq(supplier);
			Maintainer.MaintainIntersection(supplier, session);
			Refresh();

			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			Search(supplier.Name);

			Thread.Sleep(10000);
			Assert.That(Css("div.search select").SelectedItem, Is.StringEnding(supplier.Name));

			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Refresh(settings);
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(supplier.Id));
		}

		[Test, Description("Зашумлять цены для всех поставщиков")]
		public void Set_costs_noising_for_all_suppliers()
		{
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Refresh(settings);
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(0));
		}

		[Test]
		public void When_add_region_for_browse_it_must_be_in_intersection()
		{
			var region = session.Load<Region>(64UL);
			var supplier = DataMother.CreateSupplier(s => s.AddRegion(region, session));
			Save(supplier);
			Flush();

			ClickLink("Показать все регионы");
			Thread.Sleep(500);
			Assert.IsFalse(GetWorkRegion(browser, "Челябинск").Checked);
			GetWorkRegion(browser, "Челябинск").Checked = true;
			Assert.IsFalse(GetWorkRegion(browser, "Чебоксары").Checked);
			GetWorkRegion(browser, "Чебоксары").Checked = true;

			ClickButton("Сохранить");
			AssertText("Сохранено");
			var sql = @"
select
count(*)
from
Customers.Intersection i
where i.ClientId = :ClientId and i.RegionId = :RegionId
";
			var count = Convert.ToInt32(session.CreateSQLQuery(sql)
				.SetParameter("RegionId", region.Id)
				.SetParameter("ClientId", client.Id)
				.UniqueResult());
			Assert.That(count, Is.GreaterThan(0));
			count = Convert.ToInt32(session.CreateSQLQuery(sql)
				.SetParameter("RegionId", region.Id)
				.SetParameter("ClientId", client.Id)
				.UniqueResult());
			Assert.That(count, Is.GreaterThan(0));
		}

		[Test, Description("Тест снятия галки 'зашумлять цены'")]
		public void Unset_costs_noising()
		{
			var supplier = DataMother.CreateSupplier(s => { s.Payer = client.Payers.First(); });
			Save(supplier);
			client.Settings.NoiseCosts = true;
			client.Settings.NoiseCostExceptSupplier = supplier;
			session.SaveOrUpdate(client.Settings);
			Flush();

			Refresh();
			//ждем тк список для редактирования отображает js
			Assert.IsTrue(browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked);
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = false;
			Thread.Sleep(1000);
			ClickButton("Сохранить");
			AssertText("Сохранено");

			session.Refresh(settings);
			Assert.That(settings.FirmCodeOnly, Is.Null);
		}

		[Test]
		public void Set_convert_dbf_option()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.Name = "Поставщик для тестирования";
				s.FullName = "Поставщик для тестирования";
				s.AddPrice("Ассортиментный прайс", PriceType.Assortment);
			});
			Save(supplier);
			Flush();

			var checkbox = (CheckBox)Css("#drugstore_IsConvertFormat");
			checkbox.Click();
			var row = checkbox.Parents().OfType<TableRow>().First();
			((TextField)row.CssSelect("input.term")).TypeText("Поставщик для тестирования");
			Click(row, "Найти");
			Thread.Sleep(1000);
			Assert.That(((SelectList)row.CssSelect("select")).Options.Count, Is.GreaterThan(0));
			Click("Сохранить");
			AssertText("Сохранено");

			session.Refresh(settings);
			Assert.That(settings.IsConvertFormat, Is.True);
			Assert.That(settings.AssortimentPrice, Is.Not.Null);
		}

		[Test]
		public void Reset_reclame_date()
		{
			Click("Сбросить дату рекламы");
			AssertText("Сброшена");
		}

		[Test(Description = "Проверяет отображение предупреждения о том, что для конвертации не выбран ассортиментный ПЛ")]
		public void NotSetAssortmentPriceInConvert()
		{
			browser.CheckBox("drugstore_IsConvertFormat").Click();
			Click("Сохранить");
			AssertText("Сохранено");
			Click("Настройка");
			AssertText("* не указан ассортиментный прайс-лист для конвертации");
		}

		private string IsVisible(string selector)
		{
			return browser.Eval(String.Format("$(\"{0}\").is(\":visible\");", selector));
		}

		private string Error(string selector)
		{
			var label = browser.CssSelect(selector).Parent.CssSelect("label.error");
			if (label == null)
				return "";
			return label.Text;
		}

		private SelectList GetHomeRegionSelect(Browser browser)
		{
			return browser.SelectList(Find.ById("HomeRegionComboBox"));
		}

		private CheckBox GetWorkRegion(Browser browser, string name)
		{
			return ((TableCell)(browser.Table("RegionsTable").TableCell(Find.ByText(name)).NextSibling)).CheckBoxes.First();
		}

		private CheckBox GetOrderRegion(Browser browser, string name)
		{
			return ((TableCell)(browser.Table("RegionsTable").TableCell(Find.ByText(name)).NextSibling.NextSibling)).CheckBoxes.First();
		}

		private static bool UserWorkRegionExists(Browser browser, string regionName)
		{
			var labels = browser.Labels.Where(label => label != null && label.For != null && label.For.Contains("WorkRegions"));
			return labels.Any(label => label.Text.Contains(regionName));
		}

		private static bool UserOrderRegionExists(Browser browser, string regionName)
		{
			var labels = browser.Labels.Where(label => label != null && label.For != null && label.For.Contains("OrderRegions"));
			return labels.Any(label => label.Text.Contains(regionName));
		}
	}
}