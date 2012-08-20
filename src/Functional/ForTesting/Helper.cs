using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Test.Support.Web;
using WatiN.Core;
using Test.Support.Web;

namespace Functional.ForTesting
{
	public static class Helper
	{
		public static string ExtractName(Expression expression)
		{
			var unary = expression as UnaryExpression;
			if (unary != null)
				return ExtractName(unary.Operand);

			var parameter = expression as ParameterExpression;
			if (parameter != null)
				return parameter.Name;

			var member = expression as MemberExpression;
			if (member != null)
				return ExtractName(member.Expression) + "_" + member.Member.Name;

			var constant = expression as ConstantExpression;
			if (constant != null)
				return constant.Value.ToString();

			var array = expression as BinaryExpression;
			if (expression.NodeType == ExpressionType.ArrayIndex)
				return ExtractName(array.Left) + "_" + ExtractName(array.Right);

			throw new Exception(String.Format("Не знаю как разбирать выражение {0}", expression));
		}

		public static string GetElementName<T>(Expression<Func<T, object>> input)
		{
			return ExtractName(input.Body);
		}

		public static Table GetDataTable(IE ie)
		{
			return ie.Table(Find.By("ClassName", "DataTable"));
		}

		public static void AssertEquality<T>(this TableRow row, T record)
		{
			foreach (var header in FixtureMapping.Headers<T>()) {
				var cell = GetCellByHeader(row, header);
				cell.AssertEquality(record, header);
			}
		}

		public static TableCell GetCellByHeader(this TableRow row, string headerText)
		{
			var headerRow = row.ContainingTable.TableRows.First();
			var index = 0;
			foreach (var header in headerRow.Elements) {
				try {
					if (header.TagName != "TD" && header.TagName != "TH")
						continue;

					if (header.Text.Trim() == headerText)
						return row.TableCells[index];
					index++;
				}
				catch (Exception e) {
					throw new Exception(headerText, e);
				}
			}
			throw new Exception("Не нашли ячейку по заголовку " + headerText);
		}

		public static string GetHeader(TableCell cell)
		{
			return cell
				.ContainingTableRow
				.ContainingTable
				.TableRows[0]
				.Elements
				.Where(e => e.TagName == "TH")
				.Skip(cell.Index)
				.First()
				.Text;
		}

		public static void AssertEquality<T>(this TableCell cell, T record, string alias)
		{
			var value = FixtureMapping.GetValue(record, alias);
			var compare = value as Func<Element, bool>;
			if (compare != null)
				Assert.That(compare(cell.Elements.First()), Is.True, "Для ячейки {0} не нашлось значения {1} по алиасу {2}", cell, record, alias);
			else
				Assert.That(cell.Text != null ? cell.Text.Trim() : cell.Text, Is.EqualTo(value));
		}

		public static bool IsEqual<T>(this TableCell cell, T record, string alias)
		{
			var value = FixtureMapping.GetValue(record, alias);
			var compare = value as Func<Element, bool>;
			if (compare != null)
				return compare(cell.Elements.First());
			if (value == null)
				return cell.Text == null;

			return (cell.Text != null ? cell.Text.Trim() : cell.Text) == value.ToString();
		}

		public static uint GetClientCodeFromRegistrationCard(Browser browser)
		{
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
			return Convert.ToUInt32(new Regex(@"\d+").Match(browser.FindText(new Regex(@"Регистрационная карта №\s*\d+", RegexOptions.IgnoreCase))).Value);
		}

		public static uint GetLoginFromRegistrationCard(Browser browser)
		{
			return Convert.ToUInt32(new Regex(@"\d+").Match(browser.FindText(new Regex(@"Login:\s*\d+", RegexOptions.IgnoreCase))).Value);
		}

		public static IE AssertThatTableContains<T>(this IE ie, params T[] activeRecords)
		{
			var table = GetDataTable(ie);
			var dataRows = GetDataRow(table);
			Assert.That(dataRows.Count,
				Is.EqualTo(activeRecords.Length));

			var index = 0;
			foreach (var row in dataRows) {
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
			var table = GetDataTable(ie);
			foreach (var row in GetDataRow(table))
				if (row.IsEqualTo(activeRecord))
					return row;

			throw new Exception(String.Format("Не нашли строки для {0}", activeRecord));
		}

		public static void Input<T>(this IElementContainer container, Expression<Func<T, object>> input, object value)
		{
			var id = GetElementName(input);
			if (value is DateTime) {
				var calendareButton = TryFindCalendareButton(container, id);
				if (calendareButton == null)
					container.TextField(Find.ById((string)id)).Value = value.ToString();
				else
					EnterIntoCalendar(calendareButton, (DateTime)value);
			}
			else if (value is bool)
				container.CheckBox(Find.ById((string)id)).Checked = (bool)value;
			else
				container.TextField(Find.ById((string)id)).Value = value.ToString();
		}

		public static bool IsEqualTo<T>(this TableRow row, T recordBase)
		{
			foreach (var header in FixtureMapping.Headers<T>()) {
				var cell = GetCellByHeader(row, header);
				if (!cell.IsEqual(recordBase, header))
					return false;
			}
			return true;
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

		private static Button TryFindCalendareButton(IElementContainer container, string id)
		{
			var element = container.Element(Find.ById(id));
			return ((IElementContainer)element.Parent).Button(Find.ByClass("CalendarInput"));
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
	}
}