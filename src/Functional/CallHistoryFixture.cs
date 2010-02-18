using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class CallHistoryFixture : WatinFixture
	{
		[Test]
		public void TestViewCallHistory()
		{
			using (var browser = new IE(BuildTestUrl("default.aspx")))
			{
				browser.Link(Find.ByText("История звонков")).Click();
				var calendarFrom = browser.Div("beginDateCalendarHolder");
				var headerRow = calendarFrom.TableRow(Find.ByClass("headrow"));
				headerRow.TableCells[1].MouseDown();
				headerRow.TableCells[1].MouseUp(); //Выбрали предыдущий месяц
				browser.Button(Find.ByValue("Показать")).Click();

				Assert.That(browser.ContainsText("Слушать разговор"));
				Assert.That(browser.ContainsText("Дата звонка"));
				Assert.That(browser.ContainsText("Кто звонил"));
				Assert.That(browser.ContainsText("Куда звонил"));
			}
		}
	}
}
