using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using DescriptionAttribute=NUnit.Framework.DescriptionAttribute;

namespace Functional.Drugstore
{
	[TestFixture]
	public class SettingsFixture : WatinFixture2
	{
		Client client;
		DrugstoreSettings settings;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			settings = client.Settings;

			browser = Open(client, "Settings");
			Assert.That(browser.Text, Is.StringContaining("Конфигурация клиента"));
		}

		[Test]
		public void Set_buying_matrix_configuration()
		{
			browser.CheckBox("buymatrix").Click();

			browser.TextField("SearchBuymatrixText").TypeText("Фармаимпекс");
			browser.Button("SearchBuyMatrix").Click();
			Assert.That(browser.SelectList("drugstore.BuyingMatrixPriceId").SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList("drugstore.BuyingMatrixType").SelectedItem, Is.EqualTo("Черный список"));
			Assert.That(browser.SelectList("drugstore.WarningOnBuyingMatrix").SelectedItem, Is.EqualTo("Запрешать"));

			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			Assert.That(browser.SelectList("drugstore.BuyingMatrixPriceId").SelectedItem, Is.EqualTo("Фармаимпекс - Матрица"));
			Assert.That(browser.SelectList("drugstore.BuyingMatrixType").SelectedItem, Is.EqualTo("Черный список"));
			Assert.That(browser.SelectList("drugstore.WarningOnBuyingMatrix").SelectedItem, Is.EqualTo("Запрешать"));

			client.Refresh();

			Assert.That(client.Settings.BuyingMatrixPrice.Id, Is.EqualTo(4957));
			Assert.That(client.Settings.BuyingMatrixType, Is.EqualTo(BuyingMatrixType.BlackList));
			Assert.That(client.Settings.WarningOnBuyingMatrix, Is.EqualTo(BuyingMatrixAction.Block));
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

			client.Refresh();
			var user = client.Users[0];
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

			client.Refresh();
			var user = client.Users[0];
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

			client.Refresh();
			var user = client.Users[0];
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
			client.Refresh();
			var user = client.Users[0];
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
		public void ManageManualComparison()
		{
			var uri = BuildTestUrl("client/" + client.Id);
			Assert.That(browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked, Is.EqualTo(settings.ManualComparison));
			settings.ManualComparison = !settings.ManualComparison;
			settings.SaveAndFlush();
			browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked = settings.ManualComparison;
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			browser.GoTo(browser.Url);
			browser.Link(Find.ByText("Настройка")).Click();
			Assert.That(browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked, Is.EqualTo(settings.ManualComparison));
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

			var settings = DrugstoreSettings.Find(client.Id);
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

			var settings = DrugstoreSettings.Find(client.Id);
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
			var supplier = DataMother.CreateTestSupplier(s => { s.Payer = client.Payers.First(); });

			browser.Link(Find.ByText("Настройка")).Click();
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			Thread.Sleep(1000);
			browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectByValue(supplier.Id.ToString());
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			settings.Refresh();
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(supplier.Id));
		}

		[Test, Description("Зашумлять цены для всех поставщиков")]
		public void Set_costs_noising_for_all_suppliers()
		{
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			Thread.Sleep(1000);
			Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Text,
				Is.EqualTo("Зашумлять все прайс листы всех поставщиков"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Value, Is.EqualTo("0"));
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			settings.Refresh();
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(0));
		}

		[Test]
		public void When_add_region_for_browse_it_must_be_in_intersection()
		{
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
future.Intersection i
join farm.Regions r on i.RegionId = r.RegionCode and Region like :RegionName
where i.ClientId = :ClientId
";
			var count = 0;
			ArHelper.WithSession(session => count = Convert.ToInt32(session.CreateSQLQuery(sql)
													.SetParameter("RegionName", "Челябинск")
													.SetParameter("ClientId", client.Id)
													.UniqueResult()));
			Assert.That(count, Is.GreaterThan(0));
			ArHelper.WithSession(session => count = Convert.ToInt32(session.CreateSQLQuery(sql)
													.SetParameter("RegionName", "Чебоксары")
													.SetParameter("ClientId", client.Id)
													.UniqueResult()));
			Assert.That(count, Is.GreaterThan(0));
		}

		[Test, Description("Тест снятия галки 'зашумлять цены'")]
		public void Unset_costs_noising()
		{
			var supplier = DataMother.CreateTestSupplier(s => { s.Payer = client.Payers.First(); });
			client.Settings.NoiseCosts = true;
			client.Settings.NoiseCostExceptSupplier = supplier;
			client.Settings.UpdateAndFlush();

			Assert.IsTrue(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).Exists);
			Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Value,
				Is.EqualTo(client.Settings.NoiseCostExceptSupplier.Id.ToString()));
			Assert.IsTrue(browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked);
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = false;
			Thread.Sleep(1000);
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			settings.Refresh();
			Assert.That(settings.FirmCodeOnly, Is.Null);
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
			var labels = browser.Labels.Where(label => label != null && label.For.Contains("WorkRegions"));
			return labels.Any(label => label.Text.Contains(regionName));
		}

		private static bool UserOrderRegionExists(Browser browser, string regionName)
		{
			var labels = browser.Labels.Where(label => label != null && label.For.Contains("OrderRegions"));
			return labels.Any(label => label.Text.Contains(regionName));
		}
	}
}