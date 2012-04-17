using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools;
using Functional.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;

namespace Functional.Drugstore
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture2
	{
		Client testClient;

		[SetUp]
		public void Setup()
		{
			testClient = DataMother.CreateTestClientWithUser();

			Open(testClient);
			Assert.That(browser.Text, Is.StringContaining("Клиент"));
		}

		[Test]
		public void Try_to_view_orders()
		{
			browser.Link(l => l.Text == "История заказов").Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("История заказов")))
			{
				Assert.That(openedWindow.Text, Is.StringContaining("История заказов"));
			}
		}

		[Test]
		public void View_update_logs()
		{
			var user = testClient.Users.First();
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
			browser.Link(l => l.Text == "История обновлений").Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("История обновлений"))))
			{
				Assert.That(openedWindow.Text, Is.StringContaining("История обновлений"));
				Assert.That(openedWindow.Text, Is.StringContaining(user.GetLoginOrName()));
				Assert.That(openedWindow.Text, Is.StringContaining("833"));
			}
		}

		[Test]
		public void Try_to_send_message()
		{
			browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
			browser.Button(Find.ByValue("Принять")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.That(browser.Text, Is.StringContaining(String.Format("Клиент {0}, Код {1}", testClient.Name, testClient.Id)));
		}

		[Test]
		public void Try_to_search_offers()
		{
			var user = testClient.Users[0];
			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Text, Is.StringContaining("Пользователь"));
			browser.Link(Find.ByText("Поиск предложений")).Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("Поиск предложений для пользователя " + user.GetLoginOrName())))
			{
				Assert.That(openedWindow.Text, Is.StringContaining("Введите наименование или форму выпуска"));
			}
		}

		[Test]
		public void After_password_change_message_should_be_added_to_history()
		{
			ClientInfoLogEntity.MessagesForClient(testClient).Each(e => e.Delete());

			browser.Link(Find.ByText(testClient.Users[0].Login)).Click();
			browser.Link(Find.ByText("Изменить пароль")).Click();
			var title = String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", testClient.Users[0].Login, testClient.FullName);
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(title)))
			{
				openedWindow.Css("#emailsForSend").TypeText("kvasovtest@analit.net");
				openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
				openedWindow.Button(Find.ByValue("Изменить")).Click();
				Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
			}
			browser.Refresh();
			var checkText = String.Format("$$$Пользователь {0}. Бесплатное изменение пароля: Тестовое изменение пароля", testClient.Users[0].Login);
			Assert.That(browser.Text, Is.StringContaining(checkText));
		}

		[Test]
		public void Try_to_update_general_info()
		{
			browser.Input<Client>(client => client.FullName, testClient.FullName);
			browser.Input<Client>(client => client.Name, testClient.Name);
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.That(browser.Text, Is.StringContaining(String.Format("Клиент {0}, Код {1}", testClient.Name, testClient.Id)));
		}

		[Test]
		public void Open_settings()
		{
			browser.Link(Find.ByText("Настройка")).Click();
			Assert.That(browser.Text, Is.StringContaining("Конфигурация клиента"));
		}
	}
}