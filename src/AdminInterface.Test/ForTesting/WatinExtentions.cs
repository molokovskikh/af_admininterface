using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;
using WatiN.Core.Interfaces;
using TableRow=WatiN.Core.TableRow;

namespace AdminInterface.Test.ForTesting
{
    public static class WatinExtentions
    {
        public static IE AssertThatTableContains<T>(this IE ie, params T[] activeRecords)
        {
            var table = Helper.GetDataTable(ie);
            var dataRows = GetDataRow(table);
            Assert.That(dataRows.Length,
                        Is.EqualTo(activeRecords.Length));

            var index = 0;
            foreach (TableRow row in dataRows)
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
            foreach (TableRow row in GetDataRow(table))
                if (row.IsEqualTo(activeRecord))
                    return row;

            throw new Exception("Не нашли строки для {0}".Format(activeRecord));
        }

        public static void Input<T>(this IElementsContainer container, Expression<Func<T, object>> input, object value)
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

            var month = GetMonth(text.Substring(0, text.IndexOf(",")));
            string marker;
            if (month > value.Month)
                marker = "‹";
            else
                marker = "›";
            var changeMonth = calendarTable.Div(Find.ByText(marker));
            foreach (var index in Enumerable.Range(0, Math.Abs(month - value.Month)))
                SimulateClick(changeMonth);

            SimulateClick(calendarTable.TableCell(Find.ByText(value.Day.ToString())));
        }

        private static void SimulateClick(Element changeMonth)
        {
            changeMonth.FireEvent("onmousedown");
            changeMonth.FireEvent("onmouseup");
        }

        private static int GetMonth(string monthName)
        {
            return CultureInfo.GetCultureInfo("ru-Ru")
                .DateTimeFormat
                .MonthNames
                .Transform(s => s.ToLower())
                .ToList()
                .IndexOf(monthName) + 1;
        }

        private static Button TryFindCalendareButton(IElementsContainer container, string id)
        {
            var element = container.Element(Find.ById(id));
            return ((IElementsContainer) element.Parent).Button(Find.ByClass("CalendarInput"));
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
