using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core; using Test.Support.Web;
using Common.Web.Ui.Helpers;
using AdminInterface.Helpers;

namespace Functional
{
	[TestFixture, Ignore("Чинить")]
	public class UpdateLogFixture : WatinFixture2
	{
		[Test]
		public void ViewAccumulativeUpdateLog()
		{
			var updateType = UpdateType.Accumulative;
			using (var browser = ViewUpdateLogFromMainPage(updateType))
			{
				CheckColumnNames(browser, updateType);
			}
		}

		[Test]
		public void ViewCumulativeUpdateLog()
		{
			var updateType = UpdateType.Cumulative;
			using (var browser = ViewUpdateLogFromMainPage(updateType))
			{
				CheckColumnNames(browser, updateType);
			}
		}

		[Test]
		public void ViewAccessErrorUpdateLog()
		{
			var updateType = UpdateType.AccessError;
			using (var browser = ViewUpdateLogFromMainPage(updateType))
			{
				CheckColumnNames(browser, updateType);
			}
		}

		[Test]
		public void ViewServerErrorUpdateLog()
		{
			var updateType = UpdateType.ServerError;
			using (var browser = ViewUpdateLogFromMainPage(updateType))
			{
				CheckColumnNames(browser, updateType);
			}
		}		

		private void CheckColumnNames(IE browser, UpdateType updateType)
		{
			CheckCommonColumnNames(browser);
			if ((updateType == UpdateType.Accumulative) || (updateType == UpdateType.Cumulative))
			{
				Assert.That(browser.Text, Is.StringContaining("Регион"));
				Assert.That(browser.Text, Is.StringContaining("Размер приготовленных данных"));
				Assert.That(browser.Text, Is.StringContaining("Лог"));				
			}
		}

		private IE ViewUpdateLogFromMainPage(UpdateType updateType)
		{
			var uriFormat = "Logs/UpdateLog?BeginDate=01.09.2009 0:00:00&EndDate=15.01.2010 0:00:00&RegionMask=137438953471&updateType={0}";
			var uri = String.Format(uriFormat, (int)updateType);
			var browser = new IE(BuildTestUrl(uri));
			Assert.That(browser.Text, Is.StringContaining(BindingHelper.GetDescription((StatisticsType)updateType)));
			Assert.That(browser.Text, Is.StringContaining("Клиентов, отвечающих условиям выборки"));
			Assert.That(browser.Text, Is.StringContaining("Пользователь"));
			Assert.That(browser.Text, Is.StringContaining("Клиент"));

			browser.Links.Where(l => (l.Title != null) && l.Title.Equals("UserSettings")).First().Click();
			Assert.That(browser.Url, Is.StringContaining("/users/"));
			browser.Back();

			browser.Links.Where(l => (l.Title != null) && l.Title.Equals("ClientSettings")).First().Click();
			Assert.That(browser.Url, Is.StringContaining("/client/"));
			browser.Back();

			return browser;
		}

		[Test]
		public void ViewUpdateLogFromUserPage()
		{
			var uri = String.Format("Logs/UpdateLog?userId={0}", GetId(typeof(User)));			
			using (var browser = new IE(BuildTestUrl(uri)))
			{
				var calendarFrom = browser.Div("beginDateCalendarHolder");
				var headerRow = calendarFrom.TableRow(Find.ByClass("headrow"));
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp();
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp(); //Выбрали 2 месяца назад

				CheckCommonColumnNames(browser);
				Assert.That(browser.Text, Is.StringContaining("Тип обновления"));
				Assert.That(browser.Text, Is.StringContaining("Размер приготовленных данных"));
				Assert.That(browser.Text, Is.StringContaining("Лог"));
			}			
		}

		[Test]
		public void ViewUpdateLogFromClientPage()
		{
			var uri = String.Format("Logs/UpdateLog?clientCode={0}", GetId(typeof(Client)));
			using (var browser = new IE(BuildTestUrl(uri)))
			{
				var calendarFrom = browser.Div("beginDateCalendarHolder");
				var headerRow = calendarFrom.TableRow(Find.ByClass("headrow"));
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp(); 
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp(); //Выбрали 2 месяца назад

				browser.Button(Find.ByValue("Показать")).Click();

				CheckCommonColumnNames(browser);
				Assert.That(browser.Text, Is.StringContaining("Пользователь"));
				Assert.That(browser.Text, Is.StringContaining("Тип обновления"));
				Assert.That(browser.Text, Is.StringContaining("Размер приготовленных данных"));
				Assert.That(browser.Text, Is.StringContaining("Лог"));

				browser.Links.Where(l => (l.Title != null) && l.Title.Equals("UserSettings")).First().Click();
				Assert.That(browser.Url, Is.StringContaining("/users/"));
			}
		}

		private void CheckCommonColumnNames(IE browser)
		{
			Assert.That(browser.Text, Is.StringContaining("Дата"));
			Assert.That(browser.Text, Is.StringContaining("Версия"));
			Assert.That(browser.Text, Is.StringContaining("Дополнительно"));
		}

		private uint GetId(Type type)
		{
			var sqlFormat = @"
select
	{0}
from
	`future`.`Users`
where
	Id = (select max(UserId) from `logs`.`AnalitFUpdates` limit 1)";
			uint id = 0;

			if (type.Equals(typeof(User)))
			{
				var sql = String.Format(sqlFormat, "Id");
				id = Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(sql).UniqueResult()));
			}
			else if (type.Equals(typeof(Client)))
			{
				var sql = String.Format(sqlFormat, "ClientId");
				id = Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(sql).UniqueResult()));
			}
			return id;
		}

		[Test]
		public void Try_sort_by_columns()
		{
			var uri = String.Format("Logs/UpdateLog?clientCode={0}", GetId(typeof(Client)));
			using (var browser = new IE(BuildTestUrl(uri)))
			{
				var calendarFrom = browser.Div("beginDateCalendarHolder");
				var headerRow = calendarFrom.TableRow(Find.ByClass("headrow"));
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp();
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp(); //Выбрали 2 месяца назад

				browser.Button(Find.ByValue("Показать")).Click();
				Assert.That(browser.Text, Text.DoesNotContain("За указанный период клиент не обновлялся"));
				browser.Link(Find.ByText("Дата")).Click();
				Assert.That(browser.Text, Text.DoesNotContain("За указанный период клиент не обновлялся"));
				browser.Link(Find.ByText("Версия")).Click();
				Assert.That(browser.Text, Text.DoesNotContain("За указанный период клиент не обновлялся"));
				browser.Link(Find.ByText("Пользователь")).Click();
				Assert.That(browser.Text, Text.DoesNotContain("За указанный период клиент не обновлялся"));
				CheckCommonColumnNames(browser);
			}
		}
	}
}
