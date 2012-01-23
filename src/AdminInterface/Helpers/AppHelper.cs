using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Helpers
{
	public class AppHelper : Common.Web.Ui.Helpers.AppHelper
	{
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
				var period = (Period) value;
				if (period == null)
					return null;

				return "<label style='padding:2px'>Год</label>"
					+ GetEdit(name + ".Year", typeof(int), period.Year, options)
					+ "<label style='padding:2px'>Месяц</label>"
					+ GetEdit(name + ".Interval", typeof(Interval), period.Interval, options);
			});
		}

		protected override string GetBuiltinEdit(string name, Type valueType, object value, object options)
		{
			if (name.EndsWith(".Year"))
			{
				if (valueType == typeof(int))
					return helper.Select(name, Period.Years);
				else if (valueType == typeof(int?))
				{
					var items = new[] {"Все"}.Concat(Period.Years.Select(y => y.ToString())).ToArray();
					return helper.Select(name, items);
				}
			}
			return base.GetBuiltinEdit(name, valueType, value, options);
		}

		public string Liketemplate(string value)
		{
			return "$" + value;
		}

		public string Asset(string name)
		{
			var type = "";
			var extension = Path.GetExtension(name).ToLower();
			var path = "Assets/Javascripts";
			if (extension == ".js")
			{
				type = "text/javascript";
			}
			else if (extension == ".coffee")
			{
				type = "text/coffeescript";
				var compiled = Path.ChangeExtension(name, ".js");
				var file = Path.Combine(Context.ApplicationPhysicalPath, path, compiled);
				if (File.Exists(file))
				{
					type = "text/javascript";
					name = compiled;
				}
			}
			else if (extension == ".css")
				return assetHelper.Resource(new [] {name}, "Assets/Stylesheets", "<link type='text/css' rel='stylesheet' href='{0}'></link>");

			name = assetHelper.Environmentalize(Path.Combine(Context.ApplicationPhysicalPath, path), name);
			return assetHelper.Resource(new [] {name}, path, "<script type='" + type + "' src='{0}'></script>");
		}

		public override bool HavePermission(string controller, string action)
		{
			return SecurityContext.Administrator.HaveAccessTo(controller, action);
		}

		public override bool HaveEditPermission(PropertyInfo propertyInfo)
		{
			return Permission.CheckPermissionByAttribute(SecurityContext.Administrator, propertyInfo);
		}
	}
}
