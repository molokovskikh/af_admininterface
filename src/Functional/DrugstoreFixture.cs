using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using Common.Tools;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture
	{
		[Test]
		public void Try_to_send_email_notification()
		{
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
				Assert.That(browser.ContainsText("Конфигурация клиента"));
			}
		}

		[Test]
		public void Try_to_change_home_region_for_drugstore()
		{
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				var homeRegionSelect = GetHomeRegionSelect(browser);
				var changeTo = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
				homeRegionSelect.Select(changeTo);
				browser.Button(b => b.Value.Equals("Применить")).Click();
				Assert.That(browser.ContainsText("Сохранено"), Is.True);
				Assert.That(GetHomeRegionSelect(browser).SelectedOption.Text, Is.EqualTo(changeTo));

				//перезагружаем, потому что иначе увидим data bind
				browser.GoTo(browser.Url);
				browser.Refresh();
				Assert.That(GetHomeRegionSelect(browser).SelectedOption.Text, Is.EqualTo(changeTo));
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

				var workRegionMoskow = GetWorkRegion(browser, "Курск");
				Assert.That(workRegionMoskow.Checked, Is.False);
				workRegionMoskow.Checked = true;
				Assert.That(GetWorkRegion(browser, "Воронеж").Checked, Is.True);

				var orderRegionMoskow = GetOrderRegion(browser, "Курск");
				Assert.That(orderRegionMoskow.Checked, Is.False);
				orderRegionMoskow.Checked = true;
				Assert.That(GetOrderRegion(browser, "Воронеж").Checked, Is.True);

				browser.Button(b => b.Value == "Применить").Click();
				Assert.That(browser.ContainsText("Сохранено"), Is.True);

				Assert.That(GetWorkRegion(browser, "Воронеж").Checked, Is.True);
				Assert.That(GetWorkRegion(browser, "Курск").Checked, Is.True);
				Assert.That(GetOrderRegion(browser, "Воронеж").Checked, Is.True);
				Assert.That(GetOrderRegion(browser, "Курск").Checked, Is.True);


				//перезагружаем, потому что иначе увидим data bind
				browser.GoTo(browser.Url);
				browser.Refresh();
				Assert.That(GetWorkRegion(browser, "Воронеж").Checked, Is.True);
				Assert.That(GetWorkRegion(browser, "Курск").Checked, Is.True);
				Assert.That(GetOrderRegion(browser, "Воронеж").Checked, Is.True);
				Assert.That(GetOrderRegion(browser, "Курск").Checked, Is.True);

			}
		}

		private CheckBox GetWorkRegion(IE browser, string name)
		{
			return ((TableCell) browser.Table("ctl00_MainContentPlaceHolder_WRList").Label(l => l.Text.Trim() == name).Parent).CheckBoxes.First();
		}

		private CheckBox GetOrderRegion(IE browser, string name)
		{
			return ((TableCell) browser.Table("ctl00_MainContentPlaceHolder_OrderList").Label(Find.ByText(name)).Parent).CheckBoxes.First();
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
				using (var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format("История обновлений клиента {0}", client.Name))))
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
			using (var browser = new IE(BuildTestUrl("client/3616")))
			{
				browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
				browser.Button(Find.ByValue("Принять")).Click();
				Assert.That(browser.Text, Text.Contains("Сохранено"));
				Assert.That(browser.Uri.ToString(), Text.EndsWith("client/3616"));
			}
		}

		[Test]
		public void Try_to_search_offers()
		{
			using (var browser = new IE(BuildTestUrl("client/3616")))
			{
				browser.Link(Find.ByText("Поиск предложений")).Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle("Поиск предложений для клиента ТестерК2")))
				{
					Assert.That(openedWindow.Text, Text.Contains("Введите наименование или форму выпуска"));
				}
			}
		}

		[Test]
		public void After_password_change_message_should_be_added_to_history()
		{
			ClientInfoLogEntity.MessagesForClient(Client.Find(3616u)).Each(e => e.Delete());

			using(var browser = Open("client/3616"))
			{
				browser.Link(Find.ByText("KvasovT")).Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle("Изменение пароля пользователя KvasovT [Клиент: ТестерК2]")))
				{
					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Text.Contains("Пароль успешно изменен"));
				}

				browser.Refresh();
				Assert.That(browser.Text, Text.Contains("$$$Пользователь KvasovT. Платное изменение пароля: Тестовое изменение пароля"));
			}
		}

		[Test]
		public void Try_to_update_general_info()
		{
			using (var browser = new IE(BuildTestUrl("client/3616")))
			{
				browser.Input<Client>(client => client.FullName, "ТестерК2");
				browser.Input<Client>(client => client.Name, "ТестерК2");
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Text.Contains("Сохранено"));
				Assert.That(browser.Uri.ToString(), Text.EndsWith("client/3616"));
			}
		}


		private SelectList GetHomeRegionSelect(IE browser)
		{
			return (SelectList)browser.Label(l => l.Text.Contains("Домашний регион")).NextSibling;
		}
	}
}