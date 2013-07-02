using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord.Framework.Internal;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Helpers
{
	public class AppHelper : Common.Web.Ui.Helpers.AppHelper
	{
		public string FormBlockTemplate;

		public AppHelper()
		{
			RegisterEditor();
		}

		public AppHelper(IEngineContext engineContext) : base(engineContext)
		{
			RegisterEditor();
		}

		public void RegisterEditor()
		{
			Editors.Add(typeof(Period), (name, value, options) => {
				var period = (Period)value;
				if (period == null)
					return null;

				return "<label style='padding:2px'>Год</label>"
					+ GetEdit(name + ".Year", typeof(int), period.Year, options)
						+ "<label style='padding:2px'>Месяц</label>"
							+ GetEdit(name + ".Interval", typeof(Interval), period.Interval, options);
			});
		}

		public string Validation(string target)
		{
			return GetValidationError(target);
		}

		public string FormBlock(string target)
		{
			if (String.IsNullOrEmpty(FormBlockTemplate))
				return null;

			if (!Context.Services.ViewEngineManager.HasTemplate(FormBlockTemplate))
				return null;

			using (var writer = new StringWriter()) {
				var context = new ControllerContext {
					Helpers = ControllerContext.Helpers
				};
				context.PropertyBag["target"] = target;
				Context.Services.ViewEngineManager.ProcessPartial(FormBlockTemplate, writer, Context, Controller, context);
				return writer.ToString();
			}
		}

		protected override string GetBuiltinEdit(string name, Type valueType, object value, object options, PropertyInfo propertyInfo)
		{
			if (name.EndsWith(".Year")) {
				if (valueType == typeof(int))
					return helper.Select(name, Period.Years);
				else if (valueType == typeof(int?)) {
					var items = new[] { "Все" }.Concat(Period.Years.Select(y => y.ToString())).ToArray();
					return helper.Select(name, items);
				}
			}
			return base.GetBuiltinEdit(name, valueType, value, options, propertyInfo);
		}

		public string Liketemplate(string value)
		{
			return "$" + value;
		}

		public override bool HavePermission(string controller, string action)
		{
			return SecurityContext.Administrator.HaveAccessTo(controller, action);
		}

		public override bool HaveEditPermission(PropertyInfo propertyInfo)
		{
			return Permission.CheckPermissionByAttribute(SecurityContext.Administrator, propertyInfo);
		}

		public string SearchEdit(string target, IDictionary attributes)
		{
			return SearchEditV1(target, attributes);
		}

		public string SearchEditV1(string target, IDictionary attributes)
		{
			string dependOn = "";
			if (attributes != null && attributes.Contains("data-depend-on")) {
				dependOn = "data-depend-on=\"" + attributes["data-depend-on"] + "\"";
			}
			var result = new StringBuilder();
			var label = GetLabel(target);
			if (!String.IsNullOrEmpty(label)) {
				attributes = attributes ?? new Dictionary<string, string>();
				if (!attributes.Contains("data-search-title"))
					attributes.Add("data-search-title", label);
			}

			var property = FindProperty(target);
			var hiddenTarget = target;
			if (property != null) {
				var primaryKey = GetPrimaryKey(property.PropertyType);
				if (primaryKey != null) {
					hiddenTarget = target + "." + primaryKey.Property.Name;
				}
			}
			var hidden = helper.HiddenField(hiddenTarget, attributes);

			result.Append(hidden);
			var value = ObtainValue(target);
			if (value != null) {
				var labelTag = Label(target, ": ");
				result.AppendFormat("<div class=\"value\" {2}>{0} {1}</div>", labelTag, SafeHtmlEncode(value.ToString()), dependOn);
			}
			return result.ToString();
		}

		public string SearchEditV2(string target, IDictionary attributes)
		{
			var result = new StringBuilder();

			var property = FindProperty(target);
			var hiddenTarget = target;
			if (property != null) {
				var primaryKey = GetPrimaryKey(property.PropertyType);
				if (primaryKey != null) {
					hiddenTarget = target + "." + primaryKey.Property.Name;
				}
			}
			return result
				.Append("<div class=\"search-editor-v2\" ")
				.Append(GetAttributes(attributes))
				.Append(">")
				.Append("<div data-bind=\"template: template\"></div>")
				.Append(helper.HiddenField(hiddenTarget, new Dictionary<string, string> {
					{ "data-bind", "value: value" },
					{ "data-text", SafeHtmlEncode((ObtainValue(target) ?? "").ToString()) },
					{ "data-label", GetLabel(target) },
				}))
				.Append("</div>")
				.ToString();
		}

		private static PrimaryKeyModel GetPrimaryKey(Type type)
		{
			var model = ActiveRecordModel.GetModel(type);
			if (model == null)
				return null;
			var primaryKey = model.PrimaryKey;
			if (primaryKey == null) {
				return GetPrimaryKey(model.Type.BaseType);
			}
			return primaryKey;
		}

		public string ExportLink(string name, string action, object filter, IDictionary querystring)
		{
			if (filter is Sortable) {
				var query = ((Sortable)filter).PublicPropertiesToUrlParts("filter");
				foreach (var key in querystring.Keys) {
					query.Add(key.ToString(), querystring[key]);
				}
				var controller = Context.CurrentControllerContext.ControllerDescriptor.ControllerDescriptor.Name;
				return LinkTo(name, controller, action, query);
			}
			return string.Empty;
		}

		public string EditSelect(string name, object options = null, object attributes = null)
		{
			var htmlOptions = GetSelectOptions(options);
			var selectName = name + IdSufix;
			var value = (dynamic)ObtainValue(name);
			if(value is Payer)
				return EmptyableSelect(selectName, ((Payer)value).Id.ToString(), htmlOptions, attributes as IDictionary, "");

			return EmptyableSelect(selectName, value, htmlOptions, attributes as IDictionary, "");
		}
	}
}
