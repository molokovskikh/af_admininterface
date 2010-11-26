using System;
using System.Collections;
using System.IO;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;

namespace AdminInterface.Helpers
{
	public class AppHelper : AbstractHelper
	{
		public string LinkTo(object item)
		{
			var idProperty = item.GetType().GetProperty("Id");
			var id = idProperty.GetValue(item, null);
			var nameProperty = item.GetType().GetProperty("Value");
			var name = nameProperty.GetValue(item, null);
			var clazz = "";
			if (!((Address)item).Enabled)
				clazz = "DisabledByBilling";
			var uri = LinkHelper.GetVirtualDir(Context) + String.Format("/deliveries/{0}/edit", id);
			return String.Format("<a class='{1}' href='{2}'>{0}</a>", name, clazz, uri);
		}

		public string JS(params string[] items)
		{
			return Resource(items,
				"JavaScript",
				"<script type='text/javascript' src='{0}'></script>");
		}

		public string CSS(params string[] items)
		{
			return Resource(items,
				"CSS",
				"<link type='text/css' rel='stylesheet' href='{0}'></link>");
		}

		private string Resource(string[] items, string path, string template)
		{
			var result = new StringBuilder();
			foreach (var item in items)
			{
				string timeStamp;
				string itemPath;
				if (item.StartsWith("/"))
				{
					itemPath = LinkHelper.GetVirtualDir(Context) + item;
					timeStamp = GetTimeStamp(Path.Combine(Context.ApplicationPhysicalPath, item));
				}
				else
				{
					timeStamp = GetTimeStamp(Path.Combine(Path.Combine(Context.ApplicationPhysicalPath, path), item));
					itemPath = LinkHelper.GetVirtualDir(Context) + "/" + path + "/" + item ;
				}

				result.AppendFormat(template, itemPath + "?" + timeStamp);
			}
			return result.ToString();
		}

		private string GetTimeStamp(string path)
		{
			return new FileInfo(path)
				.LastWriteTime.ToString("yyyyMMddHHmmss");
		}

		public string Sortable(string name, string key)
		{
			var sorted = false;
			if (ControllerContext.PropertyBag.Contains("SortBy"))
				sorted = ControllerContext.PropertyBag["SortBy"].ToString()
					.Equals(key, StringComparison.OrdinalIgnoreCase);

			var direction = "asc";

			if (sorted &&
				ControllerContext.PropertyBag["Direction"].ToString().Equals("asc", StringComparison.OrdinalIgnoreCase))
				direction = "desc";

			var uri = String.Format("{0}/client/{1}?SortBy={2}&Direction={3}", 
				LinkHelper.GetVirtualDir(Context),
				((Client)ControllerContext.PropertyBag["Client"]).Id,
				key, direction);

			var clazz = "";
			if (sorted)
				clazz = "sort " + direction;

			return String.Format("<a href='{1}' class='{2}'>{0}</a>", name, uri, clazz);
		}

		public string FilterFor(string name)
		{
			var value = GetValue(name);
			var input = new StringBuilder();
			if (value is IEnumerable)
			{
				EmptyableSelect(name, input, value);
			}
			if (value is DatePeriod)
			{
				PeriodCalendar(input, value);
			}
			return input.ToString();
		}

		private void PeriodCalendar(StringBuilder input, object value)
		{
			input.AppendFormat(@"
<tr>
	<td></td>
	<td style='text-align:center;' class='filter-label'>С</td>
	<td style='text-align:center;' class='filter-label'>По</td>
</tr>
<tr>
	<td valign='top' class='filter-label'>Выберите период:</td>
	<td>
		{0}
		<div class=calendar id='beginDateCalendarHolder'></div>
	</td>
	<td>
		{1}
		<div class=calendar id='endDateCalendarHolder'></div>
	</td>
</tr>", 
				String.Format("<input type='hidden' id='beginDate' name='filter.Period.Begin' value='{0}'>", ((DatePeriod)value).Begin.ToShortDateString()),
				String.Format("<input type='hidden' id='endDate' name='filter.Period.End' value='{0}'>", ((DatePeriod)value).End.ToShortDateString()));
		}

		private void EmptyableSelect(string name, StringBuilder input, object value)
		{
			var label = "";
			if (name.EndsWith("Addresses"))
			{
				label = "Выберите адрес:";
			}
			if (name.EndsWith("Users"))
			{
				label = "Выберите пользователя:";
			}
			var currentValueName = "";
			if (name.EndsWith("es"))
			{
				currentValueName = name.Substring(0, name.Length - 2);
			}
			else if (name.EndsWith("s"))
			{
				currentValueName = name.Substring(0, name.Length - 1);
			}
			var currentValue = GetValue(currentValueName);
			object currentId = null;
			if (currentValue != null)
			{
				currentId = currentValue.GetType().GetProperty("Id").GetValue(currentValue, null);
			}
			var selectName = currentValueName + ".Id";;
			input.Append("<tr>");
			input.Append("<td class='filter-label'>");
			input.Append(label);
			input.Append("</td>");
			input.Append("<td colspan=2>");
			input.AppendFormat("<select name='{0}'>", selectName);
			input.Append("<option>Все</option>");
			foreach (var item in (IEnumerable)value)
			{
				var id = item.GetType().GetProperty("Id").GetValue(item, null);
				var itemName = item.GetType().GetProperty("Name").GetValue(item, null);
				if (Equals(id, currentId))
					input.AppendFormat("<option value={0} selected>{1}</option>", id, itemName);
				else
					input.AppendFormat("<option value={0}>{1}</option>", id, itemName);
			}
			input.Append("</select>");
			input.Append("</td>");
			input.Append("</tr>");
		}

		public string BeginFormFor(object filter)
		{
			return @"
<div>
	<form method=get>
		<table class='CenterBlock' border='0'>
";
		}

		public string EndFormFor(object filter)
		{
			var helper = new FormHelper(Context);
			return String.Format(@"
			<tr>
				<td></td>
				<td style='text-align:right;' colspan=2>
					{0}
				</td>
			</tr>
		</table>
	</form>
</div>
", helper.Submit("Показать"));
		}

		private object GetValue(string name)
		{
			var indexOf = name.IndexOf(".");
			var key = name.Substring(0, indexOf);
			var property = name.Substring(indexOf + 1, name.Length - indexOf - 1);
			var value = ControllerContext.PropertyBag[key];
			var propertyInfo = value.GetType().GetProperty(property);
			return propertyInfo.GetValue(value, null);
		}
	}
}