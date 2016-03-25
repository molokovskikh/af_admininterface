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
using AdminInterface.Models.Suppliers;
using NHibernate.Linq;

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

		public void BatchEdit(uint[] ids)
		{
			var suppliers = DbSession.Query<Supplier>().Where(x => ids.Contains(x.Id)).OrderBy(x => x.Name)
				.ToArray().Distinct().ToArray();
			var batchEdit = new BatchEdit();
			if (suppliers.Length == 0) {
				Error("Не выбрано ни одного поставщика для редактирования");
				RedirectToReferrer();
				return;
			}
			PropertyBag["suppliers"] = suppliers;
			PropertyBag["data"] = batchEdit;
			if (IsPost) {
				Bind(batchEdit, "data");
				if (IsValid(batchEdit)) {
					batchEdit.Update(suppliers);
					Notify("Сохранено");
					RedirectToAction("Search");
				}
			}
		}
	}
}