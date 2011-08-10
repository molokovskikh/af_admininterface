using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Castle.Components.Validator;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;

namespace AdminInterface.Helpers
{
	public class AppHelper : Common.Web.Ui.Helpers.AppHelper
	{
		public AppHelper()
		{}

		public AppHelper(IEngineContext engineContext) : base(engineContext)
		{}

		public string Liketemplate(string value)
		{
			return "$" + value;
		}

		public string Asset(string name)
		{
			var compiled = Path.ChangeExtension(name, ".js");
			var file = Path.Combine(Context.ApplicationPhysicalPath, "Assets/JavaScript", compiled);
			if (File.Exists(file))
				name = compiled;

			var type = "";
			if (Path.GetExtension(name).ToLower() == ".js")
				type = "text/javascript";
			else if (Path.GetExtension(name).ToLower() == ".coffee")
				type = "text/coffeescript";

			return Resource(new [] {name}, "Assets/JavaScript", "<script type='" + type + "' src='{0}'></script>");
		}

		public string Style(object item)
		{
			if (item is IEnablable)
			{
				if (!((IEnablable)item).Enabled)
					return "DisabledByBilling";
			}
			return "";
		}

		public override string LinkTo(object item, object title, string action)
		{
			if (item == null)
				return "";

			var controller = GetControllerName(item);

			var contributor = item as IUrlContributor;
			if (contributor != null)
			{
				var parameters = contributor.GetQueryString();
				if (parameters.Contains("controller"))
					parameters["controller"] = GetControllerName(parameters["controller"].ToString());
				return UrlHelper.Link(title.ToString(), new Dictionary<string, object>{{"params", parameters}});
			}

			if (!HavePermission(controller, action))
				return String.Format("<a href='#' class='NotAllowedLink'>{0}</a>", title);

			var clazz = Style(item);
			var uri = GetUrl(item, action);
			return String.Format("<a class='{1}' href='{2}'>{0}</a>", title, clazz, uri);
		}

		public override bool HavePermission(string controller, string action)
		{
			return SecurityContext.Administrator.HaveAccessTo(controller, action);
		}
	}
}