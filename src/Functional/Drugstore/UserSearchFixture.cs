using System;
using AdminInterface.Models;
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
	public class UserSearchFixture : WatinFixture
	{
		private void TestSearchResultsByUserInfo(IE browser, string columnName, string searchBy)
		{
			var sql = String.Format(@"select max({0}) from future.Users", columnName);
			TestSearchResults(browser, columnName, searchBy, sql);
		}

		private void TestSearchResultsByClientInfo(IE browser, string columnName, string searchBy)
		{
			var sql = String.Format(@"select max({0}) from future.Clients", columnName);
			TestSearchResults(browser, columnName, searchBy, sql);
		}

		private void TestSearchResultsByBillingInfo(IE browser, string columnName, string searchBy)
		{
			var sql = String.Format(@"
select 
	max(Payers.{0}) 
from billing.Payers 
	join future.Clients on Clients.PayerId = Payers.PayerID
	join future.users on Clients.Id = Users.ClientId", columnName);
			TestSearchResults(browser, columnName, searchBy, sql);
		}

		private void TestSearchResults(IE browser, string columnName, string searchBy, string sql)
		{
			var text = String.Empty;
			ArHelper.WithSession(session => text = session.CreateSQLQuery(sql).UniqueResult().ToString());
			browser.RadioButton(Find.ById(searchBy)).Checked = true;
			browser.TextField(Find.ById("SearchText")).TypeText(text);
			browser.Button(Find.ByValue("Поиск")).Click();

			if (browser.TableBody(Find.ById("SearchResults")).Exists)
			{
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
			else
			{
				CheckThatIsUserPage(browser);
			}
		}

		private void CheckThatIsUserPage(IE browser)
		{
			Assert.That(browser.Text, Is.StringContaining("Пользователь"));
			Assert.That(browser.Text, Is.StringContaining("Сообщения пользователя"));
			Assert.That(browser.Text, Is.StringContaining("Доступ к адресам доставки"));
		}

		[Test]
		public void DefaultSearch()
		{
			using (var browser = Open("default.aspx"))
			{
				browser.Link(Find.ByText("Поиск пользователей")).Click();
				Assert.That(browser.Text, Is.StringContaining("Поиск пользователей"));
				Assert.That(browser.Text, Is.StringContaining("Введите текст для поиска"));
				browser.Button(Find.ByValue("Поиск")).Click();
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void SearchByUserId()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByUserInfo(browser, "Id", "SearchByUserId");
			}
		}

		[Test]
		public void SearchByLogin()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByUserInfo(browser, "Login", "SearchByLogin");
				if (browser.TableBody(Find.ById("SearchResults")).Exists)
				{
					Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.EqualTo(1));
				}
				else
				{
					CheckThatIsUserPage(browser);
				}
			}
		}

		[Test]
		public void SearchByUserName()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByUserInfo(browser, "Name", "SearchByUserName");
			}
		}

		[Test]
		public void SearchByClientName()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByClientInfo(browser, "Name", "SearchByClientName");
			}
		}

		[Test]
		public void SearchByClientId()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByClientInfo(browser, "Id", "SearchByClientId");
			}
		}

		[Test]
		public void SearchByJuridicalName()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByBillingInfo(browser, "JuridicalName", "SearchByJuridicalName");
			}			
		}

		[Test]
		public void SearchByPayerId()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByBillingInfo(browser, "PayerId", "SearchByPayerId");
			}
		}

		[Test]
		public void SearchWithFilterByRegion()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.SelectList(Find.ByName("SearchBy.RegionId")).Select("Воронеж");
				browser.Button(Find.ByValue("Поиск")).Click();
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				foreach (TableRow row in browser.TableBody(Find.ById("SearchResults")).TableRows)
				{
					Assert.That(row.TableCells[6].Text, Is.EqualTo("Воронеж"));
				}
			}
		}

		[Test]
		public void SearchWithFilterBySegment()
		{
			using (var scope = new TransactionScope())
			{
				var client = DataMother.CreateTestClientWithUser();
				client.Segment = Segment.Retail;
				client.Update();
				scope.VoteCommit();
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.SelectList(Find.ByName("SearchBy.Segment")).Select("Розница");
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
		}

		[Test]
		public void SearchWithNoResults()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText("1234567890qweasdzxc][p/.,';l");
				browser.Button(Find.ByValue("Поиск")).Click();				
				Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));

				browser.TextField(Find.ById("SearchText")).TypeText("'%test%'");
				browser.Button(Find.ByValue("Поиск")).Click();
				Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void Autosearch_by_client_name()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText("фармаимп");
				browser.Button(Find.ByValue("Поиск")).Click();

				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void Autosearch_by_client_id()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText("7160");
				browser.Button(Find.ByValue("Поиск")).Click();

				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void Search_by_client_id_with_text()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText("text");
				browser.RadioButton(Find.ById("SearchByClientId")).Checked = true;
				browser.Button(Find.ByValue("Поиск")).Click();

				Assert.IsFalse(browser.TableBody(Find.ById("SearchResults")).Exists);
				Assert.That(browser.Text, Is.StringContaining("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void Search_by_client_id()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText("7160");
				browser.RadioButton(Find.ById("SearchByClientId")).Checked = true;
				browser.Button(Find.ByValue("Поиск")).Click();

				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Count, Is.GreaterThan(0));
				Assert.That(browser.Text, Is.Not.StringContaining("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void Search_with_number_symbol()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText("аптека №151");
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Autosearch_by_contact_phone()
		{
			Client client;
			using (new SessionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].AddContactGroup();
				client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Phone, String.Format("{0}-124578", client.Id.ToString().Substring(0, 4))));
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(String.Format("{0}-124578", client.Id.ToString().Substring(0, 4)));
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Autosearch_by_contact_email()
		{
			Client client;
			using (new SessionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].AddContactGroup();
				client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Email, String.Format("test{0}@test.test", client.Id)));
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(String.Format("test{0}@test.test", client.Id));
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Autosearch_by_person()
		{
			Client client;
			using (var transaction = new TransactionScope(OnDispose.Rollback))
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].AddContactGroup();
				client.Users[0].ContactGroup.AddPerson(String.Format("testPerson{0}", client.Id));
				transaction.VoteCommit();
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(String.Format("testPerson{0}", client.Id));
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Search_by_contact_email()
		{
			Client client;
			using (new SessionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].AddContactGroup();
				client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Email, String.Format("test{0}@test.test", client.Id)));
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(String.Format("test{0}@test.test", client.Id));
				browser.RadioButton(Find.ById("SearchByContacts")).Checked = true;
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Search_by_contact_phone()
		{
			Client client;
			using (new SessionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].AddContactGroup();
				client.Users[0].ContactGroup.AddContact(new Contact(ContactType.Phone, String.Format("{0}-123456", client.Id.ToString().Substring(0, 4))));
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(String.Format("{0}-123456", client.Id.ToString().Substring(0, 4)));
				browser.RadioButton(Find.ById("SearchByContacts")).Checked = true;
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Search_by_person_name()
		{
			Client client;
			using (new SessionScope())
			{
				client = DataMother.CreateTestClientWithAddressAndUser();
				client.Users[0].AddContactGroup();
				client.Users[0].ContactGroup.AddPerson(String.Format("testPerson{0}", client.Id));
			}
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(String.Format("testPerson{0}", client.Id));
				browser.RadioButton(Find.ById("SearchByPersons")).Checked = true;
				browser.Button(Find.ByValue("Поиск")).Click();

				CheckThatIsUserPage(browser);
			}
		}

		[Test]
		public void Search_from_main_page()
		{
			var client = DataMother.CreateTestClientWithAddressAndUser();
			using (var browser = Open("Main/Index"))
			{
				browser.TextField(Find.ById("SearchText")).TypeText(client.Users[0].Login);
				browser.Button(Find.ByValue("Найти")).Click();
				CheckThatIsUserPage(browser);
			}
		}
	}
}
