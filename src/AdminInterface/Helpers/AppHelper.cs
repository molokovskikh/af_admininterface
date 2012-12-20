﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
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

		public string EditSelect(string name, object options = null)
		{
			var htmlOptions = GetSelectOptions(options);
			var selectName = name + IdSufix;
			return EmptyableSelect(selectName, "", htmlOptions, options as IDictionary, "");
		}
	}
}
