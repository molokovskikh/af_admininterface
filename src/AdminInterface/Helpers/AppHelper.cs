using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Castle.Components.Validator;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Helpers
{
	public class AppHelper : AbstractHelper
	{
		public AppHelper()
		{}

		public AppHelper(IEngineContext engineContext) : base(engineContext)
		{}

		public string Style(object item)
		{
			if (item is IEnablable)
			{
				if (!((IEnablable)item).Enabled)
					return "DisabledByBilling";
			}
			return "";
		}

		public string GetValidationError(object item, string name)
		{
			var errorSummary = ((SmartDispatcherController)Controller).Binder.GetValidationSummary(item);

			if (item is ActiveRecordValidationBase)
				errorSummary = FromBaseClass((ActiveRecordValidationBase)item);

			if (errorSummary == null)
				return "";
			var errors = errorSummary.GetErrorsForProperty(name);
			if (errors == null || errors.Length == 0)
				return "";
			return String.Format("<label class=\"error\">{0}</label>", errors.First());
		}

		private ErrorSummary FromBaseClass(ActiveRecordValidationBase item)
		{
			var errorSummary = new ErrorSummary();
			if (item.PropertiesValidationErrorMessages == null)
				return errorSummary;

			foreach (PropertyInfo property in item.PropertiesValidationErrorMessages.Keys)
				foreach (var message in (ArrayList)item.PropertiesValidationErrorMessages[property])
					errorSummary.RegisterErrorMessage(property.Name, message.ToString());

			return errorSummary;
		}

		public string Edit(string name)
		{
			var helper = new FormHelper(Context);
			var type = GetValueType(name);
			var value = GetValue(name);
			if (type == typeof(bool))
			{
				var result = new StringBuilder();
				result.Append(helper.CheckboxField(name));
				result.Append(" ");
				result.Append(GetLabel(name));
				return result.ToString();
			}
			var edit = GetEdit(name, type, value);
			if (String.IsNullOrEmpty((string) edit))
				throw new Exception(String.Format("Не знаю как показать редактор для {0} с типом {1}", name, type));
			return edit;
		}

		public string LinkTo(object item)
		{
			return LinkTo(item, "");
		}

		public string LinkTo(object item, string action)
		{
			if (item == null)
				return "";
			return LinkTo(item, ((dynamic)item).Name, action);
		}

		public string LinkTo(object item, object title, string action)
		{
			if (item == null)
				return "";

			if (!HavePermission(GetControllerName(item), action))
				return String.Format("<a href='#' class='NotAllowedLink'>{0}</a>", title);

			var clazz = "";
			if (item is Address && !((Address)item).Enabled)
				clazz = "DisabledByBilling";

			var uri = GetUrl(item, action);
			return String.Format("<a class='{1}' href='{2}'>{0}</a>", title, clazz, uri);
		}

		public string GetUrl(object item)
		{
			return GetUrl(item, null);
		}

		public string GetUrl(object item, string action = null)
		{
			var dynamicItem = ((dynamic)item);
			var id = dynamicItem.Id;
			var controller = GetControllerName(item);
			if (!String.IsNullOrEmpty(action))
				action = "/" + action;
			return String.Format("{0}/{1}/{2}{3}", LinkHelper.GetVirtualDir(Context), controller, id, action);
		}

		public static string GetShortUrl(object item, string action = null)
		{
			var dynamicItem = ((dynamic)item);
			var id = dynamicItem.Id;
			var controller = GetControllerName(item);
			if (!String.IsNullOrEmpty(action))
				action = "/" + action;
			return String.Format("{0}/{1}/{2}{3}", controller, id, action);
		}

		public string LinkTo(string title, string controller, string method)
		{
			if (!HavePermission(controller, method))
				return String.Format("<a href='#' class='NotAllowedLink'>{0}</a>", title);

			if (method.ToLower() == "index")
				method = "";

			return String.Format("<a href='{1}/{2}/{3}'>{0}</a>", title, LinkHelper.GetVirtualDir(Context), controller, method);
		}

		private static string GetControllerName(object item)
		{
			var className = NHibernateUtil.GetClass(item).Name;
			if (className == "Address")
				className = "Delivery";
			else if (className == "Client")
				return "Client";
			return Inflector.Pluralize(className);
		}

		private bool HavePermission(string controller, string action)
		{
			return SecurityContext.Administrator.HaveAccessTo(controller, action);
		}

		public string JS(params string[] items)
		{
			if (HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled)
			{
				items = items.Select(Deminifie).ToArray();
			}
			return Resource(items,
				"JavaScript",
				"<script type='text/javascript' src='{0}'></script>");
		}

		private string Deminifie(string item)
		{
			if (item.EndsWith(".min.js"))
			{
				var js = item.Replace(".min.js", ".js");
				if (File.Exists(Path.Combine(Context.ApplicationPhysicalPath, "JavaScript", js)))
					return js;
				return item;
			}
			return item;
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

			var querystring = String.Format("SortBy={0}&Direction={1}", key, direction);
			if (!String.IsNullOrEmpty(uriParams))
				querystring += "&" + uriParams;

			var parameters = new Dictionary<string, object> {
				{"querystring", querystring},
				{"encode", "false"},
			};
			if (Context.CurrentControllerContext.RouteMatch != null)
				parameters.Add("params", Context.CurrentControllerContext.RouteMatch.Parameters);

			var url = UrlHelper.For(parameters);

			var clazz = "";
			if (sorted)
				clazz = "sort " + direction;

			return String.Format("<a href='{1}' class='{2}'>{0}</a>", name, url, clazz);
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
			var result = new StringBuilder();
			var value = GetValue(name);
			var valueType = GetValueType(name);

			if (valueType == typeof(string))
				label = "Введите текст для поиска:";

			if (typeof(DatePeriod).IsAssignableFrom(valueType))
			{
				PeriodCalendar(result, value);
				return result.ToString();
			}

			var input = GetEdit(name, valueType, value);

			if (input != null)
				FilterTemplate(result, label, input);

			return result.ToString();
		}

		private string GetEdit(string name, Type valueType, dynamic value)
		{
			var helper = new FormHelper(Context);
			if (valueType == typeof(string))
			{
				return helper.TextField(name);
			}
			else if (typeof(bool).IsAssignableFrom(valueType))
			{
				return helper.CheckboxField(name);
			}
			else if (typeof(Enum).IsAssignableFrom(valueType))
			{
				var values = BindingHelper.GetDescriptionsDictionary(valueType)
					.Select(p => new Tuple<string, string>(p.Key.ToString(), p.Value))
					.ToList();

				var selectedValue = "";
				if (value != null)
					selectedValue = Convert.ToInt32(value).ToString();

				return Select(name, selectedValue, values);
			}
			else if (valueType.IsNullable())
			{
				var arguments = valueType.GetGenericArguments();
				if (arguments[0].IsEnum)
				{
					var values = BindingHelper.GetDescriptionsDictionary(arguments[0])
						.Select(p => new Tuple<string, string>(p.Key.ToString(), p.Value))
						.ToList();

					var selectedValue = "";
					if (value != null)
						selectedValue = Convert.ToInt32(value).ToString();

					return EmptyableSelect(name, selectedValue, values);
				}
			}
			else if (typeof(IEnumerable).IsAssignableFrom(valueType))
			{
				var valueName = Inflector.Singularize(name);
				var currentValue = GetValue(valueName);
				var selectedValue = "";
				if (currentValue != null)
					selectedValue = currentValue.Id.ToString();

				var items = GetSelectOptions(value);
				valueName += ".Id";
				return EmptyableSelect(valueName, selectedValue, items);
			}
			if (valueType == typeof(DateTime))
			{
				var result = new StringBuilder();
				var stringValue = (string)value.ToShortDateString();
				result.AppendFormat("<input type=text name='{0}' class='required validate-date input-date' value='{1}'>", name, stringValue);
				result.Append("<input type=button class=CalendarInput>");
				return result.ToString();
			}
			else
			{
				var method = valueType.GetMethod("All", BindingFlags.Static | BindingFlags.Public);
				var selectedValue = "";
				if (value != null)
					selectedValue = value.Id.ToString();

				if (method != null)
				{
					var all = method.Invoke(null, null);
					return EmptyableSelect(name + ".Id", selectedValue, GetSelectOptions(all));
				}
			}
			return null;
		}

		private IEnumerable<Tuple<string, string>> GetSelectOptions(object value)
		{
			return (from dynamic item in (IEnumerable)value
				let id = item.Id
				let itemName = item.Name
				select new Tuple<string, string>(id.ToString(), itemName.ToString())).ToList();
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

		private string Select(string name, object currentValue, IEnumerable<Tuple<string, string>> items)
		{
			var input = new StringBuilder();
			input.AppendFormat("<select name='{0}'>", name);
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

		public string GetLabel(string name)
		{
			var label = GetBuiltinLabel(name);
			if (label == null)
			{
				label = GetLabelFromDescription(name);
			}
			return label;
		}

		private string GetBuiltinLabel(string name)
		{
			if (name.EndsWith("Regions") || name.EndsWith("Region"))
			{
				return "Выберите регион:";
			}
			else if (name.EndsWith("Recipients") || name.EndsWith("Recipient"))
			{
				return "Выберите получателя:";
			}
			else if (name.EndsWith("Addresses"))
			{
				return "Выберите адрес:";
			}
			else if (name.EndsWith("Users"))
			{
				return "Выберите пользователя:";
			}
			else if (name.EndsWith("Period"))
			{
				return "Выберите период:";
			}
			return null;
		}

		private string GetLabelFromDescription(string name)
		{
			var property = FindProperty(name);
			if (property == null)
				return null;
			return BindingHelper.TryGetDescription(property);
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
			var property = FindProperty(name);
			if (property == null)
				throw new Exception(String.Format("Не могу найти свойство {0}", name));
			return property;
		}

		private PropertyInfo FindProperty(string name)
		{
			var indexOf = name.IndexOf(".");
			if (indexOf < 0)
				throw new Exception(name);

			var key = name.Substring(0, indexOf);
			var property = name.Substring(indexOf + 1, name.Length - indexOf - 1);
			var value = ControllerContext.PropertyBag[key];
			if (value == null)
				throw new Exception(String.Format("Can`t find {0} in property bag", key));
			return NHibernateUtil.GetClass(value).GetProperty(property);
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