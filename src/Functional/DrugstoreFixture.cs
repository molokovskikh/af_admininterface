using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using Common.Tools;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using System.Threading;

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
				using (var openedWindow = IE.AttachToIE(Find.ByTitle("История заказов клиента " + client.Name)))
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
				using (var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format("Статистика обновлений"))))
				{
					Assert.That(openedWindow.Text, Is.StringContaining("История обновлений"));
					Assert.That(openedWindow.Text, Is.StringContaining(user.Login));
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
				Assert.That(browser.Text, Text.Contains("Сохранено"));
				Assert.That(browser.Uri.ToString(), Text.EndsWith("client/" + testClient.Id));
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
				using (var openedWindow = IE.AttachToIE(Find.ByTitle("Поиск предложений для клиента " + client.Name)))
				{
					Assert.That(openedWindow.Text, Text.Contains("Введите наименование или форму выпуска"));
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
				using (var openedWindow = IE.AttachToIE(Find.ByTitle(title)))
				{					
					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.ContainsText("Пароль успешно изменен"));
				}
				browser.Refresh();
				var checkText = String.Format("$$$Пользователь {0}. Платное изменение пароля: Тестовое изменение пароля", testClient.Users[0].Login);
				Assert.That(browser.Text, Text.Contains(checkText));
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
				Assert.That(browser.Text, Text.Contains("Сохранено"));
				Assert.That(browser.Uri.ToString(), Text.EndsWith("client/" + testClient.Id));
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
				Assert.That(browser.Text, Text.Contains("Сохранено"));
				browser.GoTo(browser.Url);
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.That(browser.CheckBox(Find.ByName("drugstore.ManualComparison")).Checked, Is.EqualTo(drugstoreSettings.ManualComparison));
			}
		}
	}
}