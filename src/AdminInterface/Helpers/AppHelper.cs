using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.ActiveRecord.Framework.Internal;
using Castle.MonoRail.Framework.Helpers;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Helpers
{
	public class AppHelper : AbstractHelper
	{
		public string LinkTo(object item)
		{
			return LinkTo(item, "");
		}

		public string LinkTo(object item, string action)
		{
			return LinkTo(item, ((dynamic)item).Name, action);
		}

		public string LinkTo(object item, object label, string action)
		{
			var clazz = "";
			if (item is Address && !((Address)item).Enabled)
				clazz = "DisabledByBilling";

			var uri = GetUrl(item, action);
			return String.Format("<a class='{1}' href='{3}{2}'>{0}</a>", label, clazz, uri, LinkHelper.GetVirtualDir(Context));
		}

		public static string GetUrl(object item, string action = null)
		{
			var dynamicItem = ((dynamic)item);
			var id = dynamicItem.Id;
			var className = NHibernateUtil.GetClass(item).Name;
			if (className == "Address")
			{
				className = "Delivery";
			}
			if (!String.IsNullOrEmpty(action))
			{
				action = "/" + action;
			}
			var controller = Inflector.Pluralize(className);
			return String.Format("/{0}/{1}{2}", controller, id, action);
		}

		public string LinkTo(string title, string controller, string method)
		{
			if (!HavePermission(controller, method))
				return String.Format("<a href='#' class='NotAllowedLink'>{0}</a>", title);

			if (method.ToLower() == "index")
				method = "";

			return String.Format("<a href='{1}/{2}/{3}'>{0}</a>", title, LinkHelper.GetVirtualDir(Context), controller, method);
		}

		private bool HavePermission(string controller, string action)
		{
			return SecurityContext.Administrator.HaveAccessTo(controller, action);
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
			if (ControllerContext.PropertyBag["SortBy"] != null)
				sorted = ControllerContext.PropertyBag["SortBy"].ToString()
					.Equals(key, StringComparison.OrdinalIgnoreCase);

			var direction = "asc";

			if (sorted &&
				ControllerContext.PropertyBag["Direction"].ToString().Equals("asc", StringComparison.OrdinalIgnoreCase))
				direction = "desc";

			var uriParams = "";
			if (ControllerContext.PropertyBag["filter"] != null)
			{
				var contributor = ControllerContext.PropertyBag["filter"] as SortableContributor;
				if (contributor != null)
				{
					uriParams = contributor.GetUri();
				}
			}

			var uri = String.Format("{0}/{1}?SortBy={2}&Direction={3}&{4}", 
				LinkHelper.GetVirtualDir(Context),
				GetSelfUri(),
				key,
				direction,
				uriParams);

			var clazz = "";
			if (sorted)
				clazz = "sort " + direction;

			return String.Format("<a href='{1}' class='{2}'>{0}</a>", name, uri, clazz);
		}

		public string GetSelfUri()
		{
			if (ControllerContext.PropertyBag["self"] != null)
				return ControllerContext.PropertyBag["self"].ToString();

			var action = Context.CurrentControllerContext.Action;
			if (action.ToLower() == "index")
				action = "";

			return String.Format("{0}/{1}",
				Context.CurrentControllerContext.Name,
				action
			);
		}

		public string FilterFor(string name)
		{
			return FilterFor(GetLabel(name), name);
		}

		public string FilterFor(string label, string name)
		{
			var value = GetValue(name);
			var valueType = GetValueType(name);
			var input = new StringBuilder();
			if (valueType == typeof(string))
			{
				Text(input, value, name);
			}
			else if (typeof(Enum).IsAssignableFrom(valueType))
			{

			}
			else if (valueType.IsGenericType && typeof(Nullable<>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
			{
				var arguments = valueType.GetGenericArguments();
				if (arguments[0].IsEnum)
				{
					var values = BindingHelper.GetDescriptionsDictionary(arguments[0])
						.Select(p => new Tuple<string, string>(p.Key.ToString(), p.Value))
						.ToList();
					FilterTemplate(input,
						label,
						EmptyableSelect(name, value, values));
				}
			}
			else if (typeof(IEnumerable).IsAssignableFrom(valueType))
			{
				var valueName = Inflector.Singularize(name);
				var currentValue = GetValue(valueName);
				var selectedValue = "";
				if (currentValue != null)
					selectedValue = currentValue.Id.ToString();

				var items = (from dynamic item in (IEnumerable) value
					let id = item.Id
					let itemName = item.Name
					select new Tuple<string, string>(id.ToString(), itemName.ToString())).ToList();

				valueName += ".Id";
				FilterTemplate(input,
					label,
					EmptyableSelect(valueName, selectedValue, items));
			}
			else if (typeof(DatePeriod).IsAssignableFrom(valueType))
			{
				PeriodCalendar(input, value);
			}
			else if (typeof(bool).IsAssignableFrom(valueType))
			{
				var helper = new FormHelper(Context);
				FilterTemplate(input,
					label,
					helper.CheckboxField(name)
				);
			}
			return input.ToString();
		}

		private void Text(StringBuilder input, object value, string name)
		{
			FilterTemplate(input,
				"Введите текст для поиска:",
				String.Format("<input type=text name={0} value='{1}'>", name, value));
		}

		private void FilterTemplate(StringBuilder result, string label, string input)
		{
			result.Append("<tr>");
			result.Append("<td class='filter-label'>");
			result.Append(label);
			result.Append("</td>");
			result.Append("<td colspan=2>");
			result.Append(input);
			result.Append("</td>");
			result.Append("</tr>");
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

		private string EmptyableSelect(string name, object currentValue, IEnumerable<Tuple<string, string>> items)
		{
			var input = new StringBuilder();
			input.AppendFormat("<select name='{0}'>", name);
			input.Append("<option>Все</option>");
			foreach (var item in items)
			{
				if (Equals(item.Item1, currentValue))
					input.AppendFormat("<option value={0} selected>{1}</option>", item.Item1, item.Item2);
				else
					input.AppendFormat("<option value={0}>{1}</option>", item.Item1, item.Item2);
			}
			input.Append("</select>");
			return input.ToString();
		}

		private string GetLabel(string name)
		{
			string label = "";
			if (name.EndsWith("Region"))
			{
				label = "Выберите регион:";
			}
			else if (name.EndsWith("Recipient"))
			{
				label = "Выберите получателя:";
			}
			else if (name.EndsWith("Addresse"))
			{
				label = "Выберите адрес:";
			}
			else if (name.EndsWith("User"))
			{
				label = "Выберите пользователя:";
			}
			else if (name.EndsWith("Period"))
			{
				label = "Выберите период:";
			}
			return label;
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

		private Type GetValueType(string name)
		{
			return GetProperty(name).PropertyType;
		}

		private PropertyInfo GetProperty(string name)
		{
			var indexOf = name.IndexOf(".");
			if (indexOf < 0)
				throw new Exception(name);
			var key = name.Substring(0, indexOf);
			var property = name.Substring(indexOf + 1, name.Length - indexOf - 1);
			var value = ControllerContext.PropertyBag[key];
			if (value == null)
				throw new Exception(String.Format("Can`t find {0} in property bag", key));
			return value.GetType().GetProperty(property);
		}

		private dynamic GetValue(string name)
		{
			var indexOf = name.IndexOf(".");
			if (indexOf < 0)
				throw new Exception(name);
			var key = name.Substring(0, indexOf);
			var property = name.Substring(indexOf + 1, name.Length - indexOf - 1);
			var value = ControllerContext.PropertyBag[key];

			var propertyInfo = GetProperty(name);
			return propertyInfo.GetValue(value, null);
		}
	}
}