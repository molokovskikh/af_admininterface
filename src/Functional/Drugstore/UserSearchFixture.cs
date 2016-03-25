using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Queries;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;
using WatiN.Core;
using Test.Support.Web;
using Common.Web.Ui.Helpers;
using Common.Tools;
using Functional.ForTesting;
using WatiN.Core.Native.Windows;

namespace Functional.Drugstore
{
	public class SearchFixture : AdmSeleniumFixture
	{
		[Test]
		public void Batch_edit()
		{
			var supplier = DataMother.CreateSupplier();
			Open("UserSearch/Search");
			AssertText("Поиск пользователей");
			Css("#filter_SearchText").SendKeys(supplier.Id.ToString());
			Click("Поиск");

			Click("Редактировать");
			browser
				.FindElementsByCssSelector(".select-col input[type=checkbox]")
				.First(x => x.GetAttribute("value") == supplier.Id.ToString()).Click();
			Click("Редактировать");

			AssertText("Групповое редактирование");
			Css("#data_FederalSupplier").SelectByText("Да");
			Click("Сохранить");
			AssertText("Сохранено");
			session.Refresh(supplier);
			Assert.IsTrue(supplier.IsFederal);
		}
	}

	public class UserSearchFixture : FunctionalFixture
	{
		private void TestSearchResultsByUserInfo(string columnName, SearchUserBy searchBy)
		{
			var sql = String.Format(@"select max({0}) from Customers.Users", columnName);
			TestSearchResults(searchBy, sql, new Client());
		}

		private void TestSearchResultsByClientInfo(string columnName, SearchUserBy searchBy)
		{
			var sql = String.Format(@"select max({0}) from Customers.Clients", columnName);
			TestSearchResults(searchBy, sql, new Client());
		}

		private void TestSearchResults(SearchUserBy searchBy, string sql, Client client)
		{
			var text = session.CreateSQLQuery(sql).UniqueResult().ToString();
			AssetSearch(searchBy, text, client);
		}

		private void AssetSearch(SearchUserBy searchBy, string text, Client client)
		{
			Search(searchBy, text);

			if (browser.Text.Contains("Поиск пользователей") && browser.Text.Contains("Введите текст для поиска:")) {
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
			}
			else {
				CheckThatIsUserPage(browser, client);
			}
		}

		private void Search(SearchUserBy searchBy, string text)
		{
			Css(String.Format("input[type='radio'][name='filter.SearchBy'][value='{0}']", (int)searchBy)).Checked = true;
			browser.TextField(Find.ById("filter_SearchText")).TypeText(text);
			ClickButton("Поиск");
		}

		private void CheckThatIsUserPage(Browser browser, Client client)
		{
			AssertText("Имя клиента");
			Assert.That(browser.Element("SearchResults").InnerHtml, Is.StringContaining(client.Name));
		}

		[SetUp]
		public void Setup()
		{
			Open("UserSearch/Search");
			AssertText("Поиск пользователей");
		}

		[Test]
		public void DefaultSearch()
		{
			Open();

			Click("Поиск пользователей");
			AssertText("Поиск пользователей");
			AssertText("Введите текст для поиска");
			ClickButton("Поиск");
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void SearchByLogin()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			Flush();

			AssetSearch(SearchUserBy.ByLogin, user.Login, client);
			if (browser.TableBody(Find.ById("SearchResults")).Exists) {
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.EqualTo(1));
			}
			else {
				CheckThatIsUserPage(browser, client);
			}
		}

		[Test]
		public void SearchByUserName()
		{
			TestSearchResultsByUserInfo("Name", SearchUserBy.ByUserName);
		}

		[Test]
		public void SearchByClientName()
		{
			var client = DataMother.CreateTestClientWithUser();
			MakeNameUniq(client);
			Flush();
			AssetSearch(SearchUserBy.ByClientName, client.Name, client);
		}

		[Test]
		public void SearchByClientId()
		{
			TestSearchResultsByClientInfo("Id", SearchUserBy.ByClientId);
		}

		[Test]
		public void SearchByJuridicalName()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers[0];
			payer.JuridicalName = payer.JuridicalOrganizations[0].Name;
			session.Save(payer);
			Flush();
			var name = payer.JuridicalName;
			AssetSearch(SearchUserBy.ByJuridicalName, name, client);
		}

		[Test]
		public void SearchByPayerId()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers[0];
			Flush();

			AssetSearch(SearchUserBy.ByPayerId, payer.Id.ToString(), client);
		}

		[Test, NUnit.Framework.Description("Потерял актуальность из-за изменившегося функционала, его заменил Work_Region_Test")]
		public void SearchWithFilterByRegion()
		{
			browser.SelectList(Find.ByName("filter.Region.Id")).Select("Воронеж");
			ClickButton("Поиск");
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
		}

		[Test]
		public void SearchWithNoResults()
		{
			browser.TextField(Find.ById("filter_SearchText")).TypeText("1234567890qweasdzxc][p/.,';l");
			ClickButton("Поиск");
			AssertText("По вашему запросу ничего не найдено");

			browser.TextField(Find.ById("filter_SearchText")).TypeText("'%test%'");
			ClickButton("Поиск");
			AssertText("По вашему запросу ничего не найдено");
		}

		[Test]
		public void Autosearch_by_client_name()
		{
			var client = DataMother.CreateTestClientWithUser();
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Name);
			ClickButton("Поиск");

			var tableBody = browser.TableBody(Find.ById("SearchResults"));
			Assert.That(tableBody.TableRows.Count, Is.GreaterThan(0));
			Assert.That(tableBody.Text, Is.StringContaining(client.Id.ToString()));
			Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Autosearch_by_client_id()
		{
			//нужно добавить еще одного пользователя что не произошел автовход
			var client = DataMother.TestClient(c => {
				c.AddUser(new User(c) { Name = "test", });
				c.AddUser(new User(c) { Name = "test", });
			});
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Id.ToString());
			ClickButton("Поиск");

			var tableBody = browser.TableBody(Find.ById("SearchResults"));
			Assert.That(tableBody.TableRows.Count, Is.GreaterThan(0));
			Assert.That(tableBody.Text, Is.StringContaining(client.Id.ToString()));

			Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Search_by_client_id_with_text()
		{
			Search(SearchUserBy.ByClientId, "text");

			Assert.IsFalse(browser.TableBody(Find.ById("SearchResults")).Exists);
			AssertText("По вашему запросу ничего не найдено");
		}

		[Test]
		public void Search_by_client_id()
		{
			//нужно добавить еще одного пользователя что не произошел автовход
			var client = DataMother.TestClient(c => {
				c.AddUser(new User(c) { Name = "test", });
				c.AddUser(new User(c) { Name = "test", });
			});
			Flush();

			Search(SearchUserBy.ByClientId, client.Id.ToString());

			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Search_with_number_symbol()
		{
			var client = DataMother.CreateTestClientWithUser();
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Users[0].Id.ToString());
			ClickButton("Поиск");

			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void Autosearch_by_contact_phone()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Phone, String.Format("{0}-124578", client.Id.ToString().Substring(0, 4))));
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("{0}-124578", client.Id.ToString().Substring(0, 4)));
			ClickButton("Поиск");

			CheckThatIsUserPage(browser, client);
		}

		[Test, Ignore("Функционал отключен")]
		public void Autosearch_by_contact_email()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var mail = String.Format("test{0}@test.test", client.Id);
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Email, mail));
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(mail);
			ClickButton("Поиск");

			CheckThatIsUserPage(browser, client);
		}

		[Test, Ignore("Функционал отключен")]
		public void Autosearch_by_person()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var person = String.Format("testPerson{0}", client.Id);
			client.Users[0].AddContactGroup();
			session.Save(client.Users[0].ContactGroup);
			client.Users[0].ContactGroup.AddPerson(person);
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(person);
			ClickButton("Поиск");

			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void Search_by_contact_email()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var mail = String.Format("test{0}@test.test", client.Id);
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Email, mail));
			Flush();

			Search(SearchUserBy.ByContacts, mail);
			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void Search_by_contact_phone()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var phone = String.Format("{0}-123456", client.Id.ToString().RightSlice(4));
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Phone, phone));
			Flush();

			Search(SearchUserBy.ByContacts, phone);
			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void Search_by_person_name()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var person = String.Format("testPerson{0}", client.Id);
			client.Users[0].AddContactGroup();
			session.Save(client.Users[0].ContactGroup);
			client.Users[0].ContactGroup.AddPerson(person);
			Flush();

			Search(SearchUserBy.ByPersons, person);
			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void Search_from_main_page()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			Open();
			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Users[0].Login);
			ClickButton("Найти");
			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void FirstTableIndicateTest()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user2 = client.AddUser(session, "testUser");
			var user = client.Users[0];
			user.SubmitOrders = true;
			user.IgnoreCheckMinOrder = true;
			user.AllowDownloadUnconfirmedOrders = true;
			session.Save(user);
			session.Save(user2);
			Open();
			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Name);
			ClickButton("Найти");
			var tr = browser.Table(Find.ByClass("DataTable")).TableRows[1];
			Assert.That(tr.ClassName, Is.StringContaining("first-table"));
			Assert.That(tr.ClassName, Is.StringContaining("allow-download-unconfirmed-orders"));
		}
	}
}