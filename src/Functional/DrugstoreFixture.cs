using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using System.Threading;
using Common.Web.Ui.Helpers;
using Castle.ActiveRecord;
using DescriptionAttribute = NUnit.Framework.DescriptionAttribute;

namespace Functional
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture
	{
		[Test]
		public void Try_to_send_email_notification()
		{
			using (var browser = new IE(BuildTestUrl(String.Format("client/DrugstoreSettings.rails?clientId=2575"))))
			{
				browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
				Assert.That(browser.ContainsText("Уведомления отправлены"));
			}
		}

		[Test]
		public void Try_to_change_home_region_for_drugstore()
		{
			using (var browser = new IE(BuildTestUrl("client/DrugstoreSettings.rails?clientId=2575")))
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
		}

		[Test]
		public void Change_work_region_and_order_region()
		{
			var client = DataMother.CreateTestClientWithUser();
			using(var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.That(browser.Text, Is.StringContaining("Конфигурация клиента"));

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
		}

		[Test, Description("При добавлении региона работы клиенту, он должен добавиться пользователю")]
		public void After_add_client_work_region_user_region_settings_must_be_update()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				var workRegion = GetWorkRegion(browser, "Курск");
				workRegion.Checked = true;
				browser.Button(b => b.Value == "Сохранить").Click();
				browser.Link(Find.ByText(client.Users[0].Login)).Click();

				Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
				Assert.IsTrue(UserWorkRegionExists(browser, "Курск"));

				Assert.IsTrue(UserOrderRegionExists(browser, "Воронеж"));
				Assert.IsFalse(UserOrderRegionExists(browser, "Курск"));

				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var user = client.Users[0];
					Assert.IsTrue((user.WorkRegionMask & 1) > 0);
					Assert.IsTrue((user.WorkRegionMask & 4) > 0);
					Assert.IsTrue((user.OrderRegionMask & 1) > 0);
					Assert.IsFalse((user.OrderRegionMask & 4) > 0);
				}
			}
		}

		[Test, Description("При удалении региона работы у клиента, он должен удалиться у пользователя")]
		public void After_remove_client_work_region_user_region_settings_must_be_update()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
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
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var user = client.Users[0];
					Assert.IsFalse((user.WorkRegionMask & 1) > 0);
					Assert.IsFalse((user.OrderRegionMask & 1) > 0);
					Assert.IsTrue((user.WorkRegionMask & 4) > 0);
				}
			}
		}

		[Test, Description("При добавлении региона заказа клиенту, он должен добавиться пользователю")]
		public void After_add_client_order_region_user_region_settings_must_be_update()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				var workRegion = GetOrderRegion(browser, "Липецк");
				workRegion.Checked = true;
				browser.Button(b => b.Value == "Сохранить").Click();
				browser.Link(Find.ByText(client.Users[0].Login)).Click();
				Assert.IsTrue(UserWorkRegionExists(browser, "Воронеж"));
				Assert.IsTrue(UserOrderRegionExists(browser, "Воронеж"));
				Assert.IsTrue(UserOrderRegionExists(browser, "Липецк"));
				// При добавлении региона заказа, автоматически должен подключаться такой же регион работы
				Assert.IsTrue(UserWorkRegionExists(browser, "Липецк"));

				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var user = client.Users[0];
					Assert.IsTrue((user.WorkRegionMask & 1) > 0);
					Assert.IsTrue((user.OrderRegionMask & 1) > 0);
					Assert.IsTrue((user.WorkRegionMask & 8) > 0);
					Assert.IsTrue((user.OrderRegionMask & 8) > 0);
				}
			}
		}

		[Test, Description("При удалении региона заказа у клиента, он должен удалиться у пользователя")]
		public void After_remove_client_order_region_user_region_settings_must_be_update()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
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
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var user = client.Users[0];
					Assert.IsTrue((user.WorkRegionMask & 1) > 0);
					Assert.IsTrue((user.OrderRegionMask & 1) > 0);
					Assert.IsTrue((user.WorkRegionMask & 4) > 0);
					Assert.IsFalse((user.OrderRegionMask & 4) > 0);
				}
			}
		}

		private static bool UserWorkRegionExists(IE browser, string regionName)
		{
			var labels = browser.Labels.Where(label => label != null && label.For.Contains("WorkRegions"));
			return labels.Any(label => label.Text.Contains(regionName));
		}

		private static bool UserOrderRegionExists(IE browser, string regionName)
		{
			var labels = browser.Labels.Where(label => label != null && label.For.Contains("OrderRegions"));
			return labels.Any(label => label.Text.Contains(regionName));
		}

		[Test]
		public void Show_all_regions()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.That(browser.Text, Is.StringContaining("Конфигурация клиента"));
				var countVisibleRegions = browser.Table("RegionsTable").TableRows.Count();
				
				browser.Link(Find.ByText("Показать все регионы")).Click();
				var countAllRegions = browser.Table("RegionsTable").TableRows.Count();
				Assert.That(countVisibleRegions, Is.LessThan(countAllRegions));

				browser.Link(Find.ByText("Показать только регионы по умолчанию")).Click();
				Thread.Sleep(500);
				var count = browser.Table("RegionsTable").TableRows.Count();
				Assert.That(count, Is.EqualTo(countVisibleRegions));
			}
		}

		[Test]
		public void Regions_must_be_checked_when_show_all()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
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
		}

		[Test]
		public void Change_common_settings()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.That(browser.Text, Is.StringContaining("Конфигурация клиента"));

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
		}

		private CheckBox GetWorkRegion(IE browser, string name)
		{
			return ((TableCell)(browser.Table("RegionsTable").TableCell(Find.ByText(name)).NextSibling)).CheckBoxes.First();
		}

		private CheckBox GetOrderRegion(IE browser, string name)
		{
			return ((TableCell)(browser.Table("RegionsTable").TableCell(Find.ByText(name)).NextSibling.NextSibling)).CheckBoxes.First();
		}

		[Test]
		public void Try_to_open_client_view()
		{
			using (var browser = Open("client/2575"))
			{
				Assert.That(browser.Text, Is.StringContaining("ТестерК"));
			}
		}

		[Test]
		public void Try_to_view_orders()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(l => l.Text == "История заказов").Click();
				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("История заказов клиента " + client.Name)))
				{
					Assert.That(openedWindow.Text, Is.StringContaining("История заказов"));
				}
			}
		}

		[Test]
		public void View_update_logs()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			var updateLogEnity = new UpdateLogEntity {
				RequestTime = DateTime.Now,
				AppVersion = 833,
				UpdateType = UpdateType.Accumulative,
				ResultSize = 1*1024*1024,
				Commit = true,
				UserName = user.Login,
				User = user,
			};
			updateLogEnity.Save();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(l => l.Text == "История обновлений").Click();
				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("Статистика обновлений"))))
				{
					Assert.That(openedWindow.Text, Is.StringContaining("История обновлений"));
					Assert.That(openedWindow.Text, Is.StringContaining(user.GetLoginOrName()));
					Assert.That(openedWindow.Text, Is.StringContaining("833"));
				}
			}
		}

		[Test]
		public void Try_to_send_message()
		{
			var testClient = DataMother.CreateTestClient();
			using (var browser = new IE(BuildTestUrl("client/" + testClient.Id)))
			{
				browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
				browser.Button(Find.ByValue("Принять")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.That(browser.Uri.ToString(), Is.StringEnding("client/" + testClient.Id));
			}
		}

		[Test]
		public void Try_to_search_offers()
		{
			var client = DataMother.CreateTestClient();
			client.Name = "Client for test offers";
			client.Update();
			using (var browser = new IE(BuildTestUrl("client/" + client.Id)))
			{
				browser.Link(Find.ByText("Поиск предложений")).Click();
				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("Поиск предложений для клиента " + client.Name)))
				{
					Assert.That(openedWindow.Text, Is.StringContaining("Введите наименование или форму выпуска"));
				}
			}
		}

		[Test]
		public void After_password_change_message_should_be_added_to_history()
		{
			var testClient = DataMother.CreateTestClientWithUser();
			testClient.FullName = "Test full name";
			testClient.Users[0].Name = "Test user for password change";
			testClient.Users[0].Update();
			testClient.Update();
			ClientInfoLogEntity.MessagesForClient(Client.Find(testClient.Id)).Each(e => e.Delete());

			using(var browser = Open("client/" + testClient.Id))
			{
				browser.Link(Find.ByText(testClient.Users[0].Login)).Click();
				browser.Link(Find.ByText("Изменить пароль")).Click();
				var title = String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", testClient.Users[0].Login, testClient.FullName);
				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(title)))
				{					
					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
				}
				browser.Refresh();
				var checkText = String.Format("$$$Пользователь {0}. Платное изменение пароля: Тестовое изменение пароля", testClient.Users[0].Login);
				Assert.That(browser.Text, Is.StringContaining(checkText));
			}
		}

		[Test]
		public void Try_to_update_general_info()
		{
			var testClient = DataMother.CreateTestClient();
			testClient.FullName = "Full name for test update general info";
			testClient.Name = "Name for test update general info";
			testClient.Update();
			using (var browser = new IE(BuildTestUrl("client/" + testClient.Id)))
			{
				browser.Input<Client>(client => client.FullName, testClient.FullName);
				browser.Input<Client>(client => client.Name, testClient.Name);
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.That(browser.Uri.ToString(), Is.StringEnding("client/" + testClient.Id));
			}
		}


		private SelectList GetHomeRegionSelect(IE browser)
		{
			return browser.SelectList(Find.ById("HomeRegionComboBox"));
		}

		[Test]
		public void ManageManualComparison()
		{
			var client = DataMother.CreateTestClient();
			var drugstoreSettings = DrugstoreSettings.Find(client.Id);
			var uri = BuildTestUrl("client/" + client.Id);
			using (var browser = new IE(uri))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.That(browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked, Is.EqualTo(drugstoreSettings.ManualComparison));
				drugstoreSettings.ManualComparison = !drugstoreSettings.ManualComparison;
				drugstoreSettings.SaveAndFlush();
				browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked = drugstoreSettings.ManualComparison;				
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				browser.GoTo(browser.Url);
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.That(browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked, Is.EqualTo(drugstoreSettings.ManualComparison));
			}
		}

		[Test]
		public void When_change_home_region_it_must_be_selected()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				var homeRegionSelect = GetHomeRegionSelect(browser);
				var newHomeRegionName = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
				homeRegionSelect.Select((newHomeRegionName));
				Thread.Sleep(500);
				Assert.IsTrue(GetWorkRegion(browser, newHomeRegionName).Checked);
				Assert.IsTrue(GetOrderRegion(browser, newHomeRegionName).Checked);
			}
		}

		[Test]
		public void When_add_region_for_browse_it_must_be_in_intersection()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
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
		}

		[Test]
		public void Test_option_ignore_new_prices()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("Client/{0}", client.Id))
			{
				var controlName = "drugstore.IgnoreNewPrices";
				browser.Link(Find.ByText("Настройка")).Click();
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
		}

		[Test]
		public void Max_weekly_orders_sum()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("Client/{0}", client.Id))
			{
				var controlName = "drugstore.MaxWeeklyOrdersSum";
				browser.Link(Find.ByText("Настройка")).Click();
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
		}

		[Test, Description("Зашумлять цены для всех поставщиков кроме одного")]
		public void Set_costs_noising_except_one_supplier()
		{
			var client = DataMother.CreateTestClient();
			var supplier = DataMother.CreateTestSupplier(s => { s.Payer = client.Payer; });
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
				Thread.Sleep(1000);
				browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectByValue(supplier.Id.ToString());
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}
			using (new SessionScope())
			{
				var settings = DrugstoreSettings.Find(client.Id);
				Assert.That(settings.FirmCodeOnly, Is.EqualTo(supplier.Id));
			}
		}

		[Test, Description("Зашумлять цены для всех поставщиков")]
		public void Set_costs_noising_for_all_suppliers()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = true;
				Thread.Sleep(1000);
				Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Text,
					Is.EqualTo("Зашумлять все прайс листы всех поставщиков"));
				Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Value, Is.EqualTo("0"));
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}
			using (new SessionScope())
			{
				var settings = DrugstoreSettings.Find(client.Id);
				Assert.That(settings.FirmCodeOnly, Is.EqualTo(0));
			}
		}

		[Test, Description("Тест снятия галки 'зашумлять цены'")]
		public void Unset_costs_noising()
		{
			var client = DataMother.CreateTestClient();
			var supplier = DataMother.CreateTestSupplier(s => { s.Payer = client.Payer; });
			using (new SessionScope())
			{
				client.Settings.NoiseCosts = true;
				client.Settings.NoiseCostExceptSupplier = supplier;
				client.Settings.UpdateAndFlush();
			}
			using (var browser = Open("Client/{0}", client.Id))
			{
				browser.Link(Find.ByText("Настройка")).Click();
				Thread.Sleep(1000);
				Assert.IsTrue(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).Exists);
				Assert.That(browser.SelectList(Find.ByName("drugstore.NoiseCostExceptSupplier.Id")).SelectedOption.Value,
					Is.EqualTo(client.Settings.NoiseCostExceptSupplier.Id.ToString()));
				Assert.IsTrue(browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked);
				browser.CheckBox(Find.ByName("drugstore.NoiseCosts")).Checked = false;
				Thread.Sleep(1000);
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}
			using (new SessionScope())
			{
				var settings = DrugstoreSettings.Find(client.Id);
				Assert.That(settings.FirmCodeOnly, Is.Null);
			}
		}

		[Test]
		public void Set_buying_matrix_configuration()
		{
			Client client;
			using (new SessionScope())
			{
				client = DataMother.CreateTestClient();
			}

			using (var browser = Open("client/{0}", client.Id))
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
			}

			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.That(client.Settings.BuyingMatrixPrice.Id, Is.EqualTo(4957));
				Assert.That(client.Settings.BuyingMatrixType, Is.EqualTo(BuyingMatrixType.BlackList));
				Assert.That(client.Settings.WarningOnBuyingMatrix, Is.EqualTo(BuyingMatrixAction.Block));
			}
		}
	}
}