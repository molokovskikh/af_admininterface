using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using AdminInterface.Models;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Helper(typeof(ADHelper)),
		Secure(PermissionType.ViewDrugstore, PermissionType.ViewSuppliers, Required = Required.AnyOf),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class UserSearchController : AdminInterfaceController
	{
		public void Search()
		{
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);

			var filter = new UserFilter(DbSession);
			PropertyBag["filter"] = filter;
			if (IsPost || Request.QueryString.Keys.Cast<string>().Any(k => k.StartsWith("filter."))) {
				BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
				var result = filter.Find();
				PropertyBag["SearchResults"] = result;
			}
		}
	}
}