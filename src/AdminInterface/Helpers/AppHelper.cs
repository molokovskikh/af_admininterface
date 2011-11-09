using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.Helpers
{
	public class AppHelper : Common.Web.Ui.Helpers.AppHelper
	{
		private Styler _styler = new Styler();

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

		public string Style(params object[] item)
		{
/*
			if (item is IEnablable)
			{
				if (!((IEnablable)item).Enabled)
					return "DisabledByBilling";
			}
*/
			return String.Join(" ", item.SelectMany(i => _styler.GetStyles(i)));
		}

		public override string LinkTo(object item, object title, string action)
		{
			return LinkTo(item, title, action, null);
		}

		public string LinkTo(object item, object title, string action, IDictionary query = null)
		{
			if (item == null)
				return "";

			var parameters = new Dictionary<string, object>();

			var contributor = item as IUrlContributor;
			if (contributor != null)
			{
				var queryString = contributor.GetQueryString();
				if (queryString.Contains("controller"))
				{
					parameters.Add("controller", ToControllerName(queryString["controller"].ToString()));
					queryString.Remove("controller");
				}
				parameters.Add("params", queryString);
			}
			else
			{
				var controller = GetControllerName(item);
				parameters.Add("controller", controller);

				var dynamicItem = ((dynamic)item);
				var id = (object)dynamicItem.Id;
				parameters.Add("params", new Dictionary<string, object>{{"id", id}});
			}

			if (parameters.ContainsKey("controller"))
			{
				if (parameters["controller"].ToString().ToLower() == "ReportAccounts".ToLower())
					parameters["controller"] = "Accounts";

				var controller = parameters["controller"].ToString();
				if (!HavePermission(controller, action))
					return String.Format("<a href='#' class='NotAllowedLink'>{0}</a>", title);
			}

			if (!String.IsNullOrEmpty(action))
				parameters.Add("action", action);

			if (query != null)
				parameters.Add("querystring", query);

			var attributes = new Dictionary<string, object>();
			var clazz = Style(item);
			if (!String.IsNullOrEmpty(clazz))
				attributes.Add("class", clazz);

			return UrlHelper.Link(title.ToString(), parameters, attributes);
		}

		public override bool HavePermission(string controller, string action)
		{
			return SecurityContext.Administrator.HaveAccessTo(controller, action);
		}
	}
}