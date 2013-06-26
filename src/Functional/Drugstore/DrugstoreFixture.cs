using System;
using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Common.Tools;
using Functional.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;
using WatiN.Core;
using Test.Support.Web;
using WatiN.Core.Native.Windows;

namespace Functional.Drugstore
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture2
	{
		private Client testClient;

		[SetUp]
		public void Setup()
		{
			testClient = DataMother.CreateTestClientWithUser();

			Open(testClient);
			AssertText("Клиент");
		}

		[Test]
		public void Try_to_view_orders()
		{
			browser.Link(l => l.Text == "История заказов").Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("История заказов"))) {
				Assert.That(openedWindow.Text, Is.StringContaining("История заказов"));
			}
		}

		[Test]
		public void View_update_logs()
		{
			var user = testClient.Users.First();
			var updateLogEnity = new UpdateLogEntity(user) {
				AppVersion = 833,
				ResultSize = 1 * 1024 * 1024,
				Commit = true,
			};
			Save(updateLogEnity);
			browser.Link(l => l.Text == "История обновлений").Click();
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("История обновлений")))) {
				Assert.That(openedWindow.Text, Is.StringContaining("История обновлений"));
				Assert.That(openedWindow.Text, Is.StringContaining(user.GetLoginOrName()));
				Assert.That(openedWindow.Text, Is.StringContaining("833"));
			}
		}

		[Test]
		public void Try_to_send_message()
		{
			browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
			ClickButton("Принять");
			AssertText("Сохранено");
			AssertText(String.Format("Клиент {0}, Код {1}", testClient.Name, testClient.Id));
		}

		[Test]
		public void Try_to_search_offers()
		{
			var user = testClient.Users[0];
			ClickLink(user.Login);
			AssertText("Пользователь");
			ClickLink("Поиск предложений");
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle("Поиск предложений для пользователя " + user.GetLoginOrName()))) {
				Assert.That(openedWindow.Text, Is.StringContaining("Введите наименование или форму выпуска"));
			}
		}

		[Test]
		public void After_password_change_message_should_be_added_to_history()
		{
			AuditRecord.DeleteAuditRecords(testClient);

			ClickLink(testClient.Users[0].Login);
			ClickLink("Изменить пароль");
			AssertText("Изменение пароля");
			Css("#emailsForSend").TypeText("kvasovtest@analit.net");
			browser.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
			ClickButton("Изменить");
			AssertText("Пароль успешно изменен");

			var checkText = String.Format("$$$Пользователь {0}. Бесплатное изменение пароля: Тестовое изменение пароля", testClient.Users[0].Login);
			AssertText(checkText);
		}

		[Test]
		public void Try_to_update_general_info()
		{
			browser.Input<Client>(client => client.FullName, testClient.FullName);
			browser.Input<Client>(client => client.Name, testClient.Name + testClient);
			ClickButton("Сохранить");
			AssertText("Сохранено");
			AssertText(String.Format("Клиент {0}, Код {1}", testClient.Name + testClient, testClient.Id));
		}

		[Test]
		public void Open_settings()
		{
			ClickLink("Настройка");
			AssertText("Конфигурация клиента");
		}

		[Test]
		public void RenameClientTest()
		{
			var client = DataMother.TestClient(s => {
				s.Name = "TestToRename";
				s.FullName = "FullTestToRename";
			});
			Save(client);
			Flush();
			Open(client);
			var clientNameEdit = browser.TextField(Find.ByValue("TestToRename"));
			clientNameEdit.Value = "test";
			Click("Сохранить");
			AssertText("В данном регионе уже существует клиент с таким именем");
			clientNameEdit = browser.TextField(Find.ByValue("test"));
			clientNameEdit.Value = "testTest" + client.Id;
			Click("Сохранить");
			AssertText("Сохранено");
			Assert.That(TestClient.Find(client.Id).Name, Is.EqualTo("testTest" + client.Id));
		}
	}
}