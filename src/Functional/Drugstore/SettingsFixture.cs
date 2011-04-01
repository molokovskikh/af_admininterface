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
			Assert.That(browser.Text, Is.StringContaining("������������ �������"));
		}

		[Test]
		public void Set_buying_matrix_configuration()
		{
			browser.CheckBox("buymatrix").Click();

			browser.TextField("SearchBuymatrixText").TypeText("�����������");
			browser.Button("SearchBuyMatrix").Click();
			Assert.That(browser.SelectList("drugstore.BuyingMatrixPriceId").SelectedItem, Is.EqualTo("����������� - �������"));
			Assert.That(browser.SelectList("drugstore.BuyingMatrixType").SelectedItem, Is.EqualTo("������ ������"));
			Assert.That(browser.SelectList("drugstore.WarningOnBuyingMatrix").SelectedItem, Is.EqualTo("���������"));

			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));

			Assert.That(browser.SelectList("drugstore.BuyingMatrixPriceId").SelectedItem, Is.EqualTo("����������� - �������"));
			Assert.That(browser.SelectList("drugstore.BuyingMatrixType").SelectedItem, Is.EqualTo("������ ������"));
			Assert.That(browser.SelectList("drugstore.WarningOnBuyingMatrix").SelectedItem, Is.EqualTo("���������"));

			client.Refresh();

			Assert.That(client.Settings.BuyingMatrixPrice.Id, Is.EqualTo(4957));
			Assert.That(client.Settings.BuyingMatrixType, Is.EqualTo(BuyingMatrixType.BlackList));
			Assert.That(client.Settings.WarningOnBuyingMatrix, Is.EqualTo(BuyingMatrixAction.Block));
		}

		[Test]
		public void Try_to_send_email_notification()
		{
			browser = Open(client, "Settings");
			browser.Button(Find.ByValue("��������� ����������� � ����������� �����������")).Click();
			Assert.That(browser.ContainsText("����������� ����������"));
		}

		[Test]
		public void Try_to_change_home_region_for_drugstore()
		{
			var homeRegionSelect = GetHomeRegionSelect(browser);
			var changeTo = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
			homeRegionSelect.Select(changeTo);
			browser.Button(b => b.Value.Equals("���������")).Click();
			Assert.That(browser.ContainsText("���������"), Is.True);
			browser.Link(Find.ByText("���������")).Click();
			var selectedText = GetHomeRegionSelect(browser).SelectedOption.Text;
			Assert.That(selectedText, Is.EqualTo(changeTo));
		}

		[Test]
		public void Change_work_region_and_order_region()
		{
			var workRegion = GetWorkRegion(browser, "�����");
			Assert.That(workRegion.Checked, Is.False);				
			workRegion.Checked = true;
			Assert.That(GetWorkRegion(browser, "�������").Checked, Is.True);

			var orderRegion = GetOrderRegion(browser, "������");
			Assert.That(orderRegion.Checked, Is.False);
			orderRegion.Checked = true;
			Assert.That(GetOrderRegion(browser, "�������").Checked, Is.True);

			browser.Button(b => b.Value == "���������").Click();
			Assert.That(browser.ContainsText("���������"), Is.True);
			browser.Link(Find.ByText("���������")).Click();

			Assert.That(GetWorkRegion(browser, "�������").Checked, Is.True);
			Assert.That(GetWorkRegion(browser, "�����").Checked, Is.True);
			Assert.That(GetWorkRegion(browser, "������").Checked, Is.True);
			Assert.That(GetOrderRegion(browser, "�������").Checked, Is.True);
			Assert.That(GetOrderRegion(browser, "������").Checked, Is.True);
		}


		[Test, Description("��� ���������� ������� ������ �������, �� ������ ���������� ������������")]
		public void After_add_client_work_region_user_region_settings_must_be_update()
		{
			var workRegion = GetWorkRegion(browser, "�����");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "���������").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();

			Assert.IsTrue(UserWorkRegionExists(browser, "�������"));
			Assert.IsTrue(UserWorkRegionExists(browser, "�����"));

			Assert.IsTrue(UserOrderRegionExists(browser, "�������"));
			Assert.IsFalse(UserOrderRegionExists(browser, "�����"));

			client.Refresh();
			var user = client.Users[0];
			Assert.IsTrue((user.WorkRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 4) > 0);
			Assert.IsTrue((user.OrderRegionMask & 1) > 0);
			Assert.IsFalse((user.OrderRegionMask & 4) > 0);
		}

		[Test, Description("��� �������� ������� ������ � �������, �� ������ ��������� � ������������")]
		public void After_remove_client_work_region_user_region_settings_must_be_update()
		{
			var workRegion = GetWorkRegion(browser, "�����");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "���������").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();
			Assert.IsTrue(UserWorkRegionExists(browser, "�������"));
			Assert.IsTrue(UserWorkRegionExists(browser, "�����"));

			browser.Back();
			browser.Link(Find.ByText("���������")).Click();
			workRegion = GetWorkRegion(browser, "�������");
			workRegion.Checked = false;
			browser.Button(b => b.Value == "���������").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();

			Assert.IsFalse(UserWorkRegionExists(browser, "�������"));
			// ��� �������� ������� ������ ������ ������������� ��������� ������ ������
			Assert.IsFalse(UserOrderRegionExists(browser, "�������"));
			Assert.IsTrue(UserWorkRegionExists(browser, "�����"));

			client.Refresh();
			var user = client.Users[0];
			Assert.IsFalse((user.WorkRegionMask & 1) > 0);
			Assert.IsFalse((user.OrderRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 4) > 0);
		}

		[Test, Description("��� ���������� ������� ������ �������, �� ������ ���������� ������������")]
		public void After_add_client_order_region_user_region_settings_must_be_update()
		{
			var workRegion = GetOrderRegion(browser, "������");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "���������").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();
			Assert.IsTrue(UserWorkRegionExists(browser, "�������"));
			Assert.IsTrue(UserOrderRegionExists(browser, "�������"));
			Assert.IsTrue(UserOrderRegionExists(browser, "������"));
			// ��� ���������� ������� ������, ������������� ������ ������������ ����� �� ������ ������
			Assert.IsTrue(UserWorkRegionExists(browser, "������"));

			client.Refresh();
			var user = client.Users[0];
			Assert.IsTrue((user.WorkRegionMask & 1) > 0);
			Assert.IsTrue((user.OrderRegionMask & 1) > 0);
			Assert.IsTrue((user.WorkRegionMask & 8) > 0);
			Assert.IsTrue((user.OrderRegionMask & 8) > 0);
		}

		[Test, Description("��� �������� ������� ������ � �������, �� ������ ��������� � ������������")]
		public void After_remove_client_order_region_user_region_settings_must_be_update()
		{
			// ����� ��������� ������ ������, ������������� ����������� ����� �� ������ ������
			var workRegion = GetOrderRegion(browser, "�����");
			workRegion.Checked = true;
			browser.Button(b => b.Value == "���������").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();
			Assert.IsTrue(UserWorkRegionExists(browser, "�������"));
			Assert.IsTrue(UserWorkRegionExists(browser, "�����"));
			Assert.IsTrue(UserOrderRegionExists(browser, "�����"));

			browser.Back();
			browser.Link(Find.ByText("���������")).Click();
			workRegion = GetOrderRegion(browser, "�����");
			workRegion.Checked = false;
			browser.Button(b => b.Value == "���������").Click();
			browser.Link(Find.ByText(client.Users[0].Login)).Click();

			Assert.IsTrue(UserWorkRegionExists(browser, "�������"));
			Assert.IsTrue(UserOrderRegionExists(browser, "�������"));
			Assert.IsFalse(UserOrderRegionExists(browser, "�����"));
			// ��� �������� ������� ������, ������ ������ ������ ����������
			Assert.IsTrue(UserWorkRegionExists(browser, "�����"));
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
				
			browser.Link(Find.ByText("�������� ��� �������")).Click();
			var countAllRegions = browser.Table("RegionsTable").TableRows.Count();
			Assert.That(countVisibleRegions, Is.LessThan(countAllRegions));

			browser.Link(Find.ByText("�������� ������ ������� �� ���������")).Click();
			Thread.Sleep(500);
			var count = browser.Table("RegionsTable").TableRows.Count();
			Assert.That(count, Is.EqualTo(countVisibleRegions));
		}

		[Test]
		public void Regions_must_be_checked_when_show_all()
		{
			var browseCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("browseRegion"));
			var orderCheckboxes = browser.CheckBoxes.Where(element => (element.Id != null) && element.Id.Contains("orderRegion"));
			browser.Link(Find.ByText("�������� ��� �������")).Click();
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
			// "�������� ������� � ���������� ����������"
			var checkBoxHide = browser.Div(Find.ById("commonSettings")).CheckBoxes[0];
			Assert.That(checkBoxHide.Checked, Is.False);
			checkBoxHide.Checked = true;
			browser.Button(b => b.Value == "���������").Click();
			Assert.That(browser.ContainsText("���������"), Is.True);

			browser.Link(Find.ByText("���������")).Click();
			checkBoxHide = browser.Div(Find.ById("commonSettings")).CheckBoxes[0];
			Assert.That(checkBoxHide.Checked, Is.True);
			// "��������� �� �������"
			var checkBoxInforoomEmployee = browser.Div(Find.ById("commonSettings")).CheckBoxes[1];
			checkBoxInforoomEmployee.Checked = true;
			browser.Button(b => b.Value == "���������").Click();
			Assert.That(browser.ContainsText("���������"), Is.True);

			browser.Link(Find.ByText("���������")).Click();
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
			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));
			browser.GoTo(browser.Url);
			browser.Link(Find.ByText("���������")).Click();
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
			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));

			var settings = DrugstoreSettings.Find(client.Id);
			Assert.IsTrue(settings.IgnoreNewPrices);

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("���������")).Click();
			Assert.IsTrue(browser.CheckBox(Find.ByName(controlName)).Checked);
			browser.CheckBox(Find.ByName(controlName)).Checked = false;
			browser.Button(Find.ByValue("���������")).Click();

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("���������")).Click();
			Assert.IsFalse(browser.CheckBox(Find.ByName(controlName)).Checked);
		}

		[Test]
		public void Max_weekly_orders_sum()
		{
			var controlName = "drugstore.MaxWeeklyOrdersSum";
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("0"));
			browser.TextField(Find.ByName(controlName)).TypeText("123456");
			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));

			var settings = DrugstoreSettings.Find(client.Id);
			Assert.That(settings.MaxWeeklyOrdersSum, Is.EqualTo(123456));

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("���������")).Click();
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("123456"));
			browser.TextField(Find.ByName(controlName)).Clear();
			browser.Button(Find.ByValue("���������")).Click();

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Link(Find.ByText("���������")).Click();
			Assert.That(browser.TextField(Find.ByName(controlName)).Text, Is.EqualTo("0"));
		}

		[Test, Description("��������� ���� ��� ���� ����������� ����� ������")]
		public void Set_costs_noising_except_one_supplier()
		{
			var supplier = DataMother.CreateTestSupplier(s => { s.Payer = client.Payers.First(); });

			browser.Link(Find.ByText("���������")).Click();
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			Thread.Sleep(1000);
			browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectByValue(supplier.Id.ToString());
			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));

			settings.Refresh();
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(supplier.Id));
		}

		[Test, Description("��������� ���� ��� ���� �����������")]
		public void Set_costs_noising_for_all_suppliers()
		{
			browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
			Thread.Sleep(1000);
			Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Text,
				Is.EqualTo("��������� ��� ����� ����� ���� �����������"));
			Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Value, Is.EqualTo("0"));
			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));

			settings.Refresh();
			Assert.That(settings.FirmCodeOnly, Is.EqualTo(0));
		}

		[Test]
		public void When_add_region_for_browse_it_must_be_in_intersection()
		{
			browser.Link(Find.ByText("�������� ��� �������")).Click();
			Thread.Sleep(500);
			Assert.IsFalse(GetWorkRegion(browser, "���������").Checked);
			GetWorkRegion(browser, "���������").Checked = true;
			Assert.IsFalse(GetWorkRegion(browser, "���������").Checked);
			GetWorkRegion(browser, "���������").Checked = true;

			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));
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
													.SetParameter("RegionName", "���������")
													.SetParameter("ClientId", client.Id)
													.UniqueResult()));
			Assert.That(count, Is.GreaterThan(0));
			ArHelper.WithSession(session => count = Convert.ToInt32(session.CreateSQLQuery(sql)
													.SetParameter("RegionName", "���������")
													.SetParameter("ClientId", client.Id)
													.UniqueResult()));
			Assert.That(count, Is.GreaterThan(0));
		}

		[Test, Description("���� ������ ����� '��������� ����'")]
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
			browser.Button(Find.ByValue("���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));

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