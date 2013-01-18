﻿using System;
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
			client.Settings.SmartOrderRules = SmartOrderRules.TestSmartOrder();
			Flush();
			settings = client.Settings;

			browser = Open(client, "Settings");
			Assert.That(browser.Text, Is.StringContaining("Конфигурация клиента"));
		}

		[Test]
		public void Set_buying_matrix_configuration()
		{
			dataMother.CreateMatrix();

			Css("#drugstore_EnableBuyingMatrix").Click();

			Search("Фармаимпекс");

			Assert.That(Css("div.search select").SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.BuyingMatrixType")).SelectedItem, Is.EqualTo("Белый список"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.BuyingMatrixAction")).SelectedItem, Is.EqualTo("Запретить заказ"));

			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			browser.Click("Настройка");

			Assert.That(browser.Text, Is.StringContaining("Фармаимпекс - Матрица"));
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
			dataMother.CreateMatrix();

			Css("#drugstore_EnableOfferMatrix").Click();

			Css(".term").TypeText("Фармаимпекс");
			Css(".search[type=button]").Click();
			Thread.Sleep(1000);
			Assert.That(Css("div.search select").SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.OfferMatrixType")).SelectedItem, Is.EqualTo("Белый список"));

			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			browser.Click("Настройка");

			Assert.That(browser.Text, Is.StringContaining("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.OfferMatrixType")).SelectedItem, Is.EqualTo("Белый список"));

			session.Refresh(client);

			Assert.That(client.Settings.OfferMatrixPrice.Name, Is.EqualTo("Матрица"));
			Assert.That(client.Settings.OfferMatrixType, Is.EqualTo(MatrixType.WhiteList));
		}

		[Test]
		public void Add_offer_matrix_exclude()
		{
			var supplier = dataMother.CreateMatrix();

			Maintainer.MaintainIntersection(client, client.Orgs().First());

			Css("#drugstore_EnableOfferMatrix").Click();

			Css(".term").TypeText("Фармаимпекс");
			Css(".search[type=button]").Click();
			Thread.Sleep(1000);
			Assert.That(Css("div.search select").SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));

			var excludes = ((Table)Css("#excludes"));
			excludes.Css("input[value='Добавить']").Click();
			excludes.Css(".term").TypeText("Фармаимпекс");
			excludes.Css(".search[type=button]").Click();
			Thread.Sleep(1000);
			Assert.That(excludes.Css("div.search select").SelectedItem, Is.StringEnding("Фармаимпекс"));

			Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Click("Настройка");

			Assert.That(browser.Text, Is.StringContaining("Фармаимпекс - Матрица"));
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
		public void Add_parser_and_assortiment_price()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.Name = "Фармаимпекс";
				s.FullName = "Фармаимпекс";
				s.AddPrice("Матрица", PriceType.Assortment);
			});
			Save(supplier);
			Maintainer.MaintainIntersection(client, client.Orgs().First());
			session.Save(new ParseAlgorithm { Name = "testParse" });
			Flush();

			Css("#drugstore_EnableSmartOrder").Click();
			Css("#drugstore_EnableSmartOrder").Click();

			Search("Фармаимпекс", "Выберите ассортиментный прайс лист");
			Assert.That(SearchRoot("Выберите ассортиментный прайс лист")
				.Css("select").SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));

			Search("testParse", "Выберите парсер");
			Assert.That(SearchRoot("Выберите парсер")
				.Css("select").SelectedItem, Is.EqualTo("testParse"));

			Click("Сохранить");
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Click("Настройка");
			Assert.That(browser.Text, Is.StringContaining("Фармаимпекс - Матрица"));
			Assert.That(browser.Text, Is.StringContaining("testParse"));
		}

		[Test]
		public void Try_to_send_email_notification()
		{
			browser = Open(client, "Settings");
			browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
			Assert.That(browser.ContainsText("Уведомления отправлены"));
		}

		[Test]
		public void Try_to_change_home_region_for_drugstore()
		{
			var homeRegionSelect = GetHomeRegionSelect(browser);
			var changeTo = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
			homeRegionSelect.Select(changeTo);
			browser.Button(b => b.Value.Equals("Сохранить")).Click();
			Assert.That(browser.ContainsText("Сохранено"), Is.True);
			browser.Link(Find.ByText("Настройка")).Click();
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
			browser.Link(Find.ByText("Настройка")).Click();

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
			browser.Link(Find.ByText(client.Users[0].Login)).Click();

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
			browser.Link(Find.ByText(client.Users[0].Login)).Click();
			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));

			browser.Back();
			browser.Link(Find.ByText("Настройка")).Click();
			workRegion = GetWorkRegion(browser, "Воронеж");
			workRegion.Checked = false;
			browser.Button(b => b.Value == "Сохранить").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();

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
			browser.Link(Find.ByText(client.Users[0].Login)).Click();
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
			browser.Link(Find.ByText(client.Users[0].Login)).Click();
			Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
			Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));
			Assert.IsTrue(UserOrderRegionExists(browser, "Курск"));

			browser.Back();
			browser.Link(Find.ByText("Настройка")).Click();
			workRegion = GetOrderRegion(browser, "Курск");
			workRegion.Checked = false;
			browser.Button(b => b.Value == "Сохранить").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();

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

			browser.Link(Find.ByText("Показать все регионы")).Click();
			var countAllRegions = browser.Table("RegionsTable").TableRows.Count();
			Assert.That(countVisibleRegions, Is.LessThan(countAllRegions));

			browser.Link(Find.ByText("Показать только регионы по умолчанию")).Click();
			Thread.Sleep(500);
			var count = browser.Table("RegionsTable").TableRows.Count();
			Assert.That(count, Is.EqualTo(countVisibleRegions));
		}

		[Test]
		public void Regions_must_be_checked_when_show_all()
		{
			var browseCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("browseRegion"));
			var orderCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("orderRegion"));
			browser.Link(Find.ByText("Показать все регионы")).Click();
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
			var checkBoxHide = browser.Div(Find.ById("commonSettings")).CheckBoxes[0];
			Assert.That(checkBoxHide.Checked, Is.False);
			checkBoxHide.Checked = true;
			browser.Button(b => b.Value == "Сохранить").Click();
			Assert.That(browser.ContainsText("Сохранено"), Is.True);

			browser.Link(Find.ByText("Настройка")).Click();
			checkBoxHide = browser.Div(Find.ById("commonSettings")).CheckBoxes[0];
			Assert.That(checkBoxHide.Checked, Is.True);
			// "Сотрудник АК Инфорум"
			var checkBoxInforoomEmployee = browser.Div(Find.ById("commonSettings")).CheckBoxes[1];
			checkBoxInforoomEmployee.Checked = true;
			browser.Button(b => b.Value == "Сохранить").Click();
			Assert.That(browser.ContainsText("Сохранено"), Is.True);

			browser.Link(Find.ByText("Настройка")).Click();
			checkBoxHide = browser.Div(Find.ById("commonSettings")).CheckBoxes[0];
			Assert.That(checkBoxHide.Checked, Is.True);
			checkBoxInforoomEmployee = browser.Div(Find.ById("commonSettings")).CheckBoxes[1];
			Assert.That(checkBoxInforoomEmployee.Checked, Is.True);
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
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			session.Refresh(client);
			Assert.IsTrue(settings.IgnoreNewPrices);

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("Настройка")).Click();
			Assert.IsTrue(browser.CheckBox(Find.ByName(controlName)).Checked);
			browser.CheckBox(Find.ByName(controlName)).Checked = false;
			browser.Button(Find.ByValue("Сохранить")).Click();

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("Настройка")).Click();
			Assert.IsFalse(browser.CheckBox(Find.ByName(controlName)).Checked);
		}

		[Test]
		public void Max_weekly_orders_sum()
		{
			var controlName = "drugstore.MaxWeeklyOrdersSum";
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("0"));
			browser.TextField(Find.ByName(controlName)).TypeText("123456");
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			session.Refresh(settings);
			Assert.That(settings.MaxWeeklyOrdersSum, Is.EqualTo(123456));

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("Настройка")).Click();
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("123456"));
			browser.TextField(Find.ByName(controlName)).Clear();
			browser.Button(Find.ByValue("Сохранить")).Click();

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("Настройка")).Click();
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("0"));
		}

		[Test, Description("Зашумлять цены для всех поставщиков кроме одного")]
		public void Set_costs_noising_except_one_supplier()
		{
			var supplier = DataMother.CreateSupplier();
			MakeNameUniq(supplier);
			Maintainer.MaintainIntersection(supplier);
			Refresh();

			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			Search(supplier.Name);

			Thread.Sleep(10000);
			Assert.That(Css("div.search select").SelectedItem, Is.StringEnding(supplier.Name));

			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			session.Refresh(settings);
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(supplier.Id));
		}

		[Test, Description("Зашумлять цены для всех поставщиков")]
		public void Set_costs_noising_for_all_suppliers()
		{
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			session.Refresh(settings);
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(0));
		}

		[Test]
		public void When_add_region_for_browse_it_must_be_in_intersection()
		{
			var region = Region.Find(64UL);
			var supplier = DataMother.CreateSupplier(s => { s.AddRegion(region); });
			Save(supplier);
			Flush();

			browser.Link(Find.ByText("Показать все регионы")).Click();
			Thread.Sleep(500);
			Assert.IsFalse(GetWorkRegion(browser, "Челябинск").Checked);
			GetWorkRegion(browser, "Челябинск").Checked = true;
			Assert.IsFalse(GetWorkRegion(browser, "Чебоксары").Checked);
			GetWorkRegion(browser, "Чебоксары").Checked = true;

			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
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
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

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