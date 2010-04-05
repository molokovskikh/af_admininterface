using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Common.Web.Ui.Helpers;

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
			browser.SelectList(Find.ById("SearchBy_SearchBy")).Select(searchBy);
			browser.TextField(Find.ById("SearchText")).TypeText(text);
			browser.Button(Find.ByValue("Поиск")).Click();
			Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
			Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
		}

		[Test]
		public void DefaultSearch()
		{
			using (var browser = Open("default.aspx"))
			{
				browser.Link(Find.ByText("Поиск пользователей")).Click();
				Assert.That(browser.Text, Text.Contains("Поиск пользователей"));
				Assert.That(browser.Text, Text.Contains("Введите текст для поиска"));
				browser.Button(Find.ByValue("Поиск")).Click();
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
		}

		[Test]
		public void SearchByUserId()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByUserInfo(browser, "Id", "Код пользователя");
			}
		}

		[Test]
		public void SearchByLogin()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByUserInfo(browser, "Login", "Логин пользователя");
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.EqualTo(1));
			}
		}

		[Test]
		public void SearchByUserName()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByUserInfo(browser, "Name", "Комментарий пользователя");
			}
		}

		[Test]
		public void SearchByClientName()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByClientInfo(browser, "Name", "Имя клиента");
			}
		}

		[Test]
		public void SearchByJuridicalName()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByBillingInfo(browser, "JuridicalName", "Юридическое имя");
			}			
		}

		[Test]
		public void SearchByPayerId()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				TestSearchResultsByBillingInfo(browser, "PayerID", "Код договора");
			}
		}

		[Test]
		public void SearchWithFilterByRegion()
		{
            using (var browser = Open("UserSearch/Search.rails"))
            {
            	browser.SelectList(Find.ByName("SearchBy.RegionId")).Select("Воронеж");
				browser.Button(Find.ByValue("Поиск")).Click();
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
				foreach (TableRow row in browser.TableBody(Find.ById("SearchResults")).TableRows)
				{
					Assert.That(row.TableCells[4].Text, Is.EqualTo("Воронеж"));
				}
            }
		}

		[Test]
		public void SearchWithFilterBySegment()
		{
			using (var browser = Open("UserSearch/Search.rails"))
			{
				browser.SelectList(Find.ByName("SearchBy.Segment")).Select("Розница");
				browser.Button(Find.ByValue("Поиск")).Click();
				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
				foreach (TableRow row in browser.TableBody(Find.ById("SearchResults")).TableRows)
				{
					Assert.That(row.TableCells[7].Text, Is.EqualTo("Розница"));
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

				Assert.That(browser.TableBody(Find.ById("SearchResults")).TableRows.Length, Is.GreaterThan(0));
				Assert.That(browser.Text, Text.DoesNotContain("По вашему запросу ничего не найдено"));
			}
		}
	}
}
