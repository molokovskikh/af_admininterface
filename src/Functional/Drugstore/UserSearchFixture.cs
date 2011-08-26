using System;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using AdminInterface.Test.ForTesting;
using Common.Web.Ui.Models;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Castle.ActiveRecord;

namespace Functional
{
	public class UserSearchFixture : WatinFixture2
	{
		private void TestSearchResultsByUserInfo(Browser browser, string columnName, SearchUserBy searchBy)
		{
			var sql = String.Format(@"select max({0}) from future.Users", columnName);
			TestSearchResults(browser, columnName, searchBy, sql);
		}

		private void TestSearchResultsByClientInfo(Browser browser, string columnName, SearchUserBy searchBy)
		{
			var sql = String.Format(@"select max({0}) from future.Clients", columnName);
			TestSearchResults(browser, columnName, searchBy, sql);
		}

		private void TestSearchResults(Browser browser, string columnName, SearchUserBy searchBy, string sql)
		{
			var text = String.Empty;
			ArHelper.WithSession(session => text = session.CreateSQLQuery(sql).UniqueResult().ToString());
			AssetSearch(browser, searchBy, text);
		}

		private void AssetSearch(Browser browser, SearchUserBy searchBy, string text)
		{
			browser.RadioButton(Find.ById("Search" + searchBy.ToString())).Checked = true;
			browser.TextField(Find.ById("filter_SearchText")).TypeText(text);
			browser.Button(Find.ByValue("Поиск")).Click();

			if (browser.Text.Contains("Поиск пользователей") && browser.Text.Contains("Введите текст для поиска:"))
			{
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
			else
			{
				CheckThatIsUserPage(browser);
			}
		}

		private void CheckThatIsUserPage(Browser browser)
		{
			Assert.That(browser.Text, Is.StringContaining("Пользователь"));
			Assert.That(browser.Text, Is.StringContaining("Сообщения пользователя"));
			Assert.That(browser.Text, Is.StringContaining("Доступ к адресам доставки"));
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
			Open("/");

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
			TestSearchResultsByUserInfo(browser, "Id", SearchUserBy.ByUserId);
			Assert.That(browser.Text, Is.StringContaining("Поиск пользователей"));
		}

		[Test]
		public void SearchByLogin()
		{
			TestSearchResultsByUserInfo(browser, "Login", SearchUserBy.ByLogin);
			if (browser.TableBody(Find.ById("SearchResults")).Exists)
			{
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.EqualTo(1));
			}
			else
			{
				CheckThatIsUserPage(browser);
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
			TestSearchResultsByClientInfo(browser, "Name", SearchUserBy.ByClientName);
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
			scope.Flush();
			var name = payer.JuridicalName;
			AssetSearch(browser, SearchUserBy.ByJuridicalName, name);
		}

		[Test]
		public void SearchByPayerId()
		{
			var client = DataMother.CreateTestClientWithUser();
			var payer = client.Payers[0];
			scope.Flush();

			AssetSearch(browser, SearchUserBy.ByPayerId, payer.Id.ToString());
		}

		[Test]
		public void SearchWithFilterByRegion()
		{
			browser.SelectList(Find.ByName("filter.Region.Id")).Select("Воронеж");
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			foreach (TableRow row in browser.TableBody(Find.ById("SearchResults")).TableRows)
			{
				Assert.That(row.TableCells[6].Text, Is.EqualTo("Воронеж"));
			}
		}

		[Test]
		public void SearchWithFilterBySegment()
		{
			var client = DataMother.CreateTestClientWithUser();
			client.Segment = Segment.Retail;
			client.Update();

			browser.SelectList(Find.ByName("filter.Segment")).Select("Розница");
			browser.Button(Find.ByValue("Поиск")).Click();
			if (browser.TableBody(Find.ById("SearchResults")).Exists)
			{
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				foreach (var row in browser.TableBody(Find.ById("SearchResults")).TableRows)
					Assert.That(row.GetCellByHeader("Сегмент").Text, Is.EqualTo("Розница"));
			}
			else
			{
				CheckThatIsUserPage(browser);
			}
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
			scope.Flush();

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
				c.AddUser(new User((Service)c) {Name = "test",});
				c.AddUser(new User((Service)c) {Name = "test",});
			});
			scope.Flush();

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
			browser.TextField(Find.ById("filter_SearchText")).TypeText("text");
			browser.RadioButton(Find.ById("SearchByClientId")).Checked = true;
			browser.Button(Find.ByValue("Поиск")).Click();

			Assert.IsFalse(browser.TableBody(Find.ById("SearchResults")).Exists);
			Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Search_by_client_id()
		{
			//нужно добавить еще одного пользователя что не произошел автовход
			var client = DataMother.TestClient(c => {
				c.AddUser(new User((Service)c) {Name = "test",});
				c.AddUser(new User((Service)c) {Name = "test",});
			});
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Id.ToString());
			browser.RadioButton(Find.ById("SearchByClientId")).Checked = true;
			browser.Button(Find.ByValue("Поиск")).Click();

			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
			Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void Search_with_number_symbol()
		{
			var client = DataMother.CreateTestClientWithUser();
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Users[0].Id.ToString());
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Autosearch_by_contact_phone()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Phone, String.Format("{0}-124578", client.Id.ToString().Substring(0, 4))));
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("{0}-124578", client.Id.ToString().Substring(0, 4)));
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Autosearch_by_contact_email()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Email, String.Format("test{0}@test.test", client.Id)));
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("test{0}@test.test", client.Id));
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Autosearch_by_person()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].SaveAndFlush();
			client.Users[0].ContactGroup.AddPerson(String.Format("testPerson{0}", client.Id));
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("testPerson{0}", client.Id));
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Search_by_contact_email()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Email, String.Format("test{0}@test.test", client.Id)));
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("test{0}@test.test", client.Id));
			browser.RadioButton(Find.ById("SearchByContacts")).Checked = true;
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Search_by_contact_phone()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Phone, String.Format("{0}-123456", client.Id.ToString().Substring(0, 4))));
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("{0}-123456", client.Id.ToString().Substring(0, 4)));
			browser.RadioButton(Find.ById("SearchByContacts")).Checked = true;
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Search_by_person_name()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			client.Users[0].AddContactGroup();
			client.Users[0].ContactGroup.SaveAndFlush();
			client.Users[0].ContactGroup.AddPerson(String.Format("testPerson{0}", client.Id));
			scope.Flush();

			browser.TextField(Find.ById("filter_SearchText")).TypeText(String.Format("testPerson{0}", client.Id));
			browser.RadioButton(Find.ById("SearchByPersons")).Checked = true;
			browser.Button(Find.ByValue("Поиск")).Click();

			CheckThatIsUserPage(browser);
		}

		[Test]
		public void Search_from_main_page()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			Open("/");
			browser.TextField(Find.ById("filter_SearchText")).TypeText(client.Users[0].Login);
			browser.Button(Find.ByValue("Найти")).Click();
			CheckThatIsUserPage(browser);
		}
	}
}
