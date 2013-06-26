using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;

namespace Functional.Drugstore
{
	public class ClientFixture : WatinFixture2
	{
		private Client client;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			Open(client);
			AssertText("Клиент");
		}

		[Test]
		public void AddLegalEntityWithEmptyName()
		{
			Click("Новое юр. лицо");
			Click("Создать");
			AssertText("Заполнение поля обязательно");
			browser.TextField(Find.ById("JuridicalOrganization_Name")).Value = "Тестовое";
			browser.TextField(Find.ById("JuridicalOrganization_FullName")).Value = "Тестовое";
			Click("Создать");
			AssertText("Юридическое лицо создано");
		}

		[Test]
		public void Try_sort_users()
		{
			var user = client.AddUser("test user");
			Refresh();
			AssertText("клиент");
			Assert.IsTrue(browser.Link(Find.ByText("Код пользователя")).Exists);
			Assert.IsTrue(browser.Link(Find.ByText("Имя пользователя")).Exists);
			Assert.That(browser.Table("users").Exists);
			// Берем 1-ю и 2-ю строки потому что 0 - это заголовок
			var login1 = Convert.ToInt64(browser.Table("users").TableRows[1].TableCells[0].Text);
			var login2 = Convert.ToInt64(browser.Table("users").TableRows[2].TableCells[0].Text);
			Assert.That(login1, Is.LessThan(login2));
			//по умолчанию мы применяем сортировку сообщений по дате сообщения, по этому нужно
			//кликнуть 2 раза первый что бы отсортировать в прямом порядке, второй в обратном что и проверяет тест
			Click("Код пользователя");
			Click("Код пользователя");
			login1 = Convert.ToInt64(browser.Table("users").TableRows[1].TableCells[0].Text);
			login2 = Convert.ToInt64(browser.Table("users").TableRows[2].TableCells[0].Text);
			Assert.That(login1, Is.GreaterThan(login2));
			ClickLink("Имя пользователя");
			Assert.That(browser.Table("users").Exists);
		}

		[Test]
		public void Check_last_service_usage()
		{
			var logs = client.Users.First().Logs;
			logs.AFTime = DateTime.Now;
			session.Save(logs);
			Refresh();

			AssertText(logs.AFTime.ToString());
		}

		[Test]
		public void View_client_message_from_user()
		{
			client.AddUser("User2");
			Refresh();

			browser.TextField(Find.ByName("message")).TypeText("This message for client");
			ClickButton("Принять");

			ClickLink(client.Users[0].Login);
			AssertText("This message for client");
			browser.TextField(Find.ByName("message")).TypeText("This message for user1");
			ClickButton("Принять");

			browser.GoTo(BuildTestUrl(String.Format("users/{0}/edit", client.Users[1].Id)));
			browser.Refresh();
			AssertText("This message for client");
			Assert.That(browser.Text, Is.Not.StringContaining("This message for user1"));
			browser.TextField(Find.ByName("message")).TypeText("This message for user2");
			ClickButton("Принять");

			browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
			browser.Refresh();
			AssertText("This message for user1");
			AssertText("This message for user2");
			AssertText("This message for client");
		}

		[Test]
		public void Sort_messages()
		{
			browser.TextField(Find.ByName("message")).TypeText("This message for client");
			ClickButton("Принять");
			ClickLink(client.Users[0].Login);
			browser.TextField(Find.ByName("message")).TypeText("This message for user1");
			ClickButton("Принять");
			Open(client);
			browser.Refresh();

			Assert.IsTrue(browser.Link(Find.ByText("Дата")).Exists);
			Assert.IsTrue(browser.Link(Find.ByText("Оператор")).Exists);
			Assert.IsTrue(browser.Link(Find.ByText("Название")).Exists);

			ClickLink("Дата");
			ClickLink("Дата");
			AssertText("This message for client");
			Assert.IsTrue(browser.Table("messages").Exists);

			ClickLink("Оператор");
			ClickLink("Оператор");
			AssertText("This message for client");
			Assert.IsTrue(browser.Table("messages").Exists);

			ClickLink("Название");
			ClickLink("Название");
			AssertText("This message for client");
			Assert.IsTrue(browser.Table("messages").Exists);
		}

		[Test]
		public void Menu_items()
		{
			var baseUrl = browser.Url;

			browser.GoTo(browser.Link(Find.ByText("История обновлений")).Url);
			Assert.AreEqual(String.Format("История обновлений клиента {0}", client.Name), browser.Title);
			browser.GoTo(baseUrl);

			browser.GoTo(browser.Link(Find.ByText("История документов")).Url);
			Assert.AreEqual("История документов", browser.Title);
			browser.GoTo(baseUrl);

			browser.GoTo(browser.Link(Find.ByText("История заказов")).Url);
			Assert.AreEqual("История заказов", browser.Title);
			browser.GoTo(baseUrl);
		}

		[Test]
		public void Legal_entity_block_view()
		{
			var organizaions = client.Payers.SelectMany(p => p.JuridicalOrganizations).ToList();
			Assert.Greater(organizaions.Count, 0);
			foreach (var legalEntity in organizaions) {
				AssertText(legalEntity.Name);
			}
		}

		[Test]
		public void Legal_entity_click()
		{
			var organizaion = client.Payers.SelectMany(p => p.JuridicalOrganizations).FirstOrDefault();
			Assert.IsNotNull(organizaion);
			organizaion.Name = "JuridicalOrganization";
			session.Save(organizaion);
			Flush();
			browser.Refresh();
			Click(organizaion.Name);
			AssertText("Краткое наименование");
			AssertText(organizaion.Name);
			browser.TextField("JuridicalOrganization_Name").AppendText("Test_JuridicalOrganization");
			Click("Сохранить");
			AssertText("Test_JuridicalOrganization");
			AssertText("Сохранено");
		}

		[Test]
		public void Create_delete_legal_entity_test()
		{
			Click("Новое юр. лицо");
			browser.TextField("JuridicalOrganization_Name").AppendText("new_JuridicalOrganization_name");
			browser.TextField("JuridicalOrganization_FullName").AppendText("new_JuridicalOrganization_FullName");
			Click("Создать");
			AssertText("Юридическое лицо создано");
			browser.Refresh();
			Open(client);
			session.Refresh(client);
			var organ = session.QueryOver<LegalEntity>().Where(e => e.Name == "new_JuridicalOrganization_name").List().Last();
			browser.Button(string.Format("deleteButton{0}", organ.Id)).Click();
			AssertText("Удалено");
		}

		[Test]
		public void Client_page_must_contains_client_id()
		{
			AssertText(String.Format("Клиент {0}, Код {1}", client.Name, client.Id));
		}

		private Payer Prepare_change_payer()
		{
			var payer = DataMother.CreatePayer();
			payer.INN = "123321123";
			payer.Save();
			payer.Name = "Тестовый плательщик " + payer.Id;
			Flush();

			Css("#ChangePayer .term").TypeText(payer.Name);
			Css("#ChangePayer input[type=button].search").Click();

			browser.WaitUntilContainsText(payer.Name, 1);
			var select = (SelectList)Css("select[name=payerId]");
			Assert.That(select.SelectedItem, Is.EqualTo(String.Format("{0}, {1}", payer.Id, payer.Name)));

			return payer;
		}

		[Test]
		public void Change_client_payer()
		{
			var payer = Prepare_change_payer();

			Css("#ChangePayer [type=submit]").Click();
			session.Refresh(payer);

			Assert.IsFalse(payer.JuridicalOrganizations.Contains(client.Orgs().First()));
			AssertText("Изменено");
			AssertText("ИНН: " + payer.INN);
			session.Refresh(client);
			Assert.That(client.Payers, Is.EquivalentTo(new[] { payer }));
		}

		[Test]
		public void Change_payer_and_jurdical_organization()
		{
			var payer = Prepare_change_payer();
			browser.CheckBox("andJurdicalOrganizationCheckbox").Checked = true;

			Css("#ChangePayer [type=submit]").Click();
			session.Refresh(payer);

			Assert.IsTrue(client.Orgs().Count() == 1);
			Assert.IsTrue(payer.JuridicalOrganizations.Contains(client.Orgs().First()));
			AssertText("ИНН: " + payer.INN);
			session.Refresh(client);
			Assert.That(client.Payers, Is.EquivalentTo(new[] { payer }));
		}

		[Test]
		public void Hide_system_messages()
		{
			AssertText("$$$Изменено");
			Css("#filter_Types_1_").Click();
			Assert.That(browser.Text, Is.Not.ContainsSubstring("$$$Изменено"));
		}
	}
}