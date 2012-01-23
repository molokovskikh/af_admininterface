using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using AdminInterface.Helpers;
using AdminInterface.Test.ForTesting;
using Common.Tools;
using NUnit.Framework;
using WatiN.Core;
using WatiN.CssSelectorExtensions;

namespace Functional.ForTesting
{
	public static class WatinExtentions
	{
		public static Button Button(this Browser browser, object item, string action)
		{
			var url = WatinFixture2.GetShortUrl(item, action);
			return (Button)browser
				.Form(l => l.GetAttributeValue("action") != null && l.GetAttributeValue("action").EndsWith(url))
				.CssSelect("input[type=submit]");
		}

		public static Link LinkFor(this Browser browser, object item, string action)
		{
			var url = WatinFixture2.GetShortUrl(item, action);
			return browser.Link(l => l.Url.EndsWith(url));
		}

		public static dynamic Css(this Browser browser, string selector)
		{
			return browser.CssSelect(selector);
		}

		public static dynamic Css(this Element browser, string selector)
		{
			return browser.CssSelect(selector);
		}

		public static IEnumerable<Element> Parents(this Element element)
		{
			var parent = element.Parent;
			while (parent != null)
			{
				yield return parent;
				parent = parent.Parent;
			}
		}

		public static void Click(this Browser browser, string name)
		{
			var button = browser.Buttons.FirstOrDefault(b => String.Equals(b.Value, name, StringComparison.CurrentCultureIgnoreCase));
			if (button != null)
			{
				button.Click();
				return;
			}
			browser.Link(Find.ByText(name)).Click();
		}

		public static void Click(this IElementContainer browser, string name)
		{
			var button = browser.Buttons.FirstOrDefault(b => String.Equals(b.Value, name, StringComparison.CurrentCultureIgnoreCase));
			if (button != null)
			{
				button.Click();
				return;
			}
			browser.Link(Find.ByText(name)).Click();
		}

		public static IE AssertThatTableContains<T>(this IE ie, params T[] activeRecords)
		{
			var table = Helper.GetDataTable(ie);
			var dataRows = GetDataRow(table);
			Assert.That(dataRows.Count,
						Is.EqualTo(activeRecords.Length));

			var index = 0;
			foreach (var row in dataRows)
			{
				row.AssertEquality(activeRecords[index]);
				index++;
			}

			return ie;
		}

		private static TableRowCollection GetDataRow(Table table)
		{
			return table.TableRows.Filter(r => r.ClassName != null && r.ClassName.Contains("Row"));
		}

		public static TableRow FindRow<T>(this IE ie, T activeRecord)
		{
			var table = Helper.GetDataTable(ie);
			foreach (var row in GetDataRow(table))
				if (row.IsEqualTo(activeRecord))
					return row;

			throw new Exception(String.Format("Не нашли строки для {0}", activeRecord));
		}

		public static void Input<T>(this IElementContainer container, Expression<Func<T, object>> input, object value)
		{
			var id = Helper.GetElementName(input);
			if (value is DateTime)
			{
				var calendareButton = TryFindCalendareButton(container, id);
				if (calendareButton == null)
					container.TextField(Find.ById(id)).Value = value.ToString();
				else
					EnterIntoCalendar(calendareButton, (DateTime) value);
			}
			else if (value is bool)
				container.CheckBox(Find.ById(id)).Checked = (bool) value;
			else 
				container.TextField(Find.ById(id)).Value = value.ToString();
		}

		private static void EnterIntoCalendar(Button button, DateTime value)
		{
			button.Click();
			var div = button.DomContainer.Div(Find.ByClass("calendar"));
			var calendarTable = div.Tables.First();
			var text = calendarTable.TableCell(Find.ByClass("title")).Text;

			var year = GetYear(text);
			var month = GetMonth(text);
			string marker;
			if (month > value.Month)
				marker = "‹";
			else
				marker = "›";
			var changeMonth = calendarTable.Div(Find.ByText(marker));

			var yearMarker = year > value.Year ? "«" : "»";
			var changeYear = calendarTable.Div(Find.ByText(yearMarker));

			foreach (var i in Enumerable.Range(0, Math.Abs(year - value.Year)))
				SimulateClick(changeYear);

			foreach (var i in Enumerable.Range(0, Math.Abs(month - value.Month)))
				SimulateClick(changeMonth);

			SimulateClick(calendarTable.TableCell(Find.ByText(value.Day.ToString())));
		}

		private static int GetYear(string title)
		{
			return Convert.ToInt32(title.Substring(title.IndexOf(",") + 1, title.Length - title.IndexOf(",") - 1).Trim());
		}

		private static void SimulateClick(Element changeMonth)
		{
			changeMonth.FireEvent("onmousedown");
			changeMonth.FireEvent("onmouseup");
		}

		private static int GetMonth(string title)
		{
			var monthName = title.Substring(0, title.IndexOf(","));
			return CultureInfo.GetCultureInfo("ru-Ru")
				.DateTimeFormat
				.MonthNames
				.Select(s => s.ToLower())
				.ToList()
				.IndexOf(monthName) + 1;
		}

		private static Button TryFindCalendareButton(IElementContainer container, string id)
		{
			var element = container.Element(Find.ById(id));
			return ((IElementContainer) element.Parent).Button(Find.ByClass("CalendarInput"));
		}

		public static bool IsEqualTo<T>(this TableRow row, T recordBase)
		{
			foreach (var header in FixtureMapping.Headers<T>())
			{
				var cell = Helper.GetCellByHeader(row, header);
				if (!cell.IsEqual(recordBase, header))
					return false;
			}
			return true;
		}
	}
}
