using System;
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
using WatiN.Core.Native.Windows;

namespace Functional.Drugstore
{
	public class UserSearchFixture : WatinFixture2
	{
		private void TestSearchResultsByUserInfo(Browser browser, string columnName, SearchUserBy searchBy)
		{
			var sql = String.Format(@"select max({0}) from Customers.Users", columnName);
			TestSearchResults(browser, columnName, searchBy, sql, new Client());
		}

		private void TestSearchResultsByClientInfo(Browser browser, string columnName, SearchUserBy searchBy)
		{
			var sql = String.Format(@"select max({0}) from Customers.Clients", columnName);
			TestSearchResults(browser, columnName, searchBy, sql, new Client());
		}

		private void TestSearchResults(Browser browser, string columnName, SearchUserBy searchBy, string sql, Client client)
		{
			var text = session.CreateSQLQuery(sql).UniqueResult().ToString();
			AssetSearch(searchBy, text, client);
		}

		private void AssetSearch(SearchUserBy searchBy, string text, Client client)
		{
			Search(searchBy, text);

			if (browser.Text.Contains("Поиск пользователей") && browser.Text.Contains("Введите текст для поиска:")) {
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
			else {
				CheckThatIsUserPage(browser, client);
			}
		}

		private void Search(SearchUserBy searchBy, string text)
		{
			Css(String.Format("input[type='radio'][name='filter.SearchBy'][value='{0}']", (int)searchBy)).Checked = true;
			browser.TextField(Find.ById("filter_SearchText")).TypeText(text);
			browser.Button(Find.ByValue("Поиск")).Click();
		}

		private void CheckThatIsUserPage(Browser browser, Client client)
		{
			Assert.That(browser.Text, Is.StringContaining("Имя клиента"));
			Assert.That(browser.Element("SearchResults").InnerHtml, Is.StringContaining(client.Name));
		}

		[SetUp]
		public void Setup()
		{
			Open("UserSearch/Search");
			Assert.That(browser.Text, Is.StringContaining("Поиск пользователей"));
		}

		[Test]
		public void DefaultSearch()
		{
			Open();

			browser.Link(Find.ByText("Поиск пользователей")).Click();
			Assert.That(browser.Text, Is.StringContaining("Поиск пользователей"));
			Assert.That(browser.Text, Is.StringContaining("Введите текст для поиска"));
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void SearchByUserId()
		{
			Assert.That(browser.Text, Is.StringContaining("Поиск пользователей"));
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
			TestSearchResultsByUserInfo(browser, "Name", SearchUserBy.ByUserName);
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
			TestSearchResultsByClientInfo(browser, "Id", SearchUserBy.ByClientId);
		}

		[Test]
		public void SearchByJuridicalName()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers[0];
			payer.JuridicalName = payer.JuridicalOrganizations[0].Name;
			payer.Update();
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
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
		}

		[Test]
		public void SearchWithNoResults()
		{
			browser.TextField(Find.ById("filter_SearchText")).TypeText("1234567890qweasdzxc][p/.,';l");
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));

			browser.TextField(Find.ById("filter_SearchText")).TypeText("'%test%'");
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Autosearch_by_client_name()
		{
			var client = DataMother.CreateTestClientWithUser();
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Name);
			browser.Button(Find.ByValue("Поиск")).Click();

			var tableBody = browser.TableBody(Find.ById("SearchResults"));
			Assert.That(tableBody.TableRows.Count, Is.GreaterThan(0));
			Assert.That(tableBody.Text, Is.StringContaining(client.Id.ToString()));
			Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
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
			browser.Button(Find.ByValue("Поиск")).Click();

			var tableBody = browser.TableBody(Find.ById("SearchResults"));
			Assert.That(tableBody.TableRows.Count, Is.GreaterThan(0));
			Assert.That(tableBody.Text, Is.StringContaining(client.Id.ToString()));

			Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Search_by_client_id_with_text()
		{
			Search(SearchUserBy.ByClientId, "text");

			Assert.IsFalse(browser.TableBody(Find.ById("SearchResults")).Exists);
			Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));
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
			browser.Button(Find.ByValue("Поиск")).Click();

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
			browser.Button(Find.ByValue("Поиск")).Click();

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
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser, client);
		}

		[Test, Ignore("Функционал отключен")]
		public void Autosearch_by_person()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var person = String.Format("testPerson{0}", client.Id);
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.Save();
			client.Users[0].ContactGroup.AddPerson(person);
			Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(person);
			browser.Button(Find.ByValue("Поиск")).Click();

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
			client.Users[0].ContactGroup.SaveAndFlush();
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
			browser.Button(Find.ByValue("Найти")).Click();
			CheckThatIsUserPage(browser, client);
		}

		[Test]
		public void FirstTableIndicateTest()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			var user2 = client.AddUser("testUser");
			var user = client.Users[0];
			user.SubmitOrders = true;
			user.IgnoreCheckMinOrder = true;
			user.AllowDownloadUnconfirmedOrders = true;
			session.Save(user);
			session.Save(user2);
			Open();
			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Name);
			browser.Button(Find.ByValue("Найти")).Click();
			var tr = browser.Table(Find.ByClass("DataTable")).TableRows[1];
			Assert.That(tr.ClassName, Is.StringContaining("first-table"));
			Assert.That(tr.ClassName, Is.StringContaining("allow-download-unconfirmed-orders"));
		}
	}
}