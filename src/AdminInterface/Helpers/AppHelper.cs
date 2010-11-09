using System;
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
	}
}