using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.EditSettings)]
	public class SettingsController : AdminInterfaceController
	{
		public SettingsController()
		{
			SetARDataBinder(AutoLoadBehavior.NewInstanceIfInvalidKey);
		}

		public void Terms()
		{
			var items = DbSession.Query<FederalSupplierToken>().OrderBy(x => x.Name).ToList();

			if (IsPost) {
				var added = BindObject<FederalSupplierToken[]>(ParamStore.Form, "items")
					.OrderBy(x => x.Name)
					.ToList();

				if (IsValid(added)) {
					var deleted = items.Where(r => added.All(n => n.Id != r.Id));
					foreach (var item in deleted)
						DbSession.Delete(item);
					foreach (var item in added)
						DbSession.Save(item);

					Notify("Сохранено");
					RedirectToReferrer();
				} else {
					items = added;
				}
			}
			PropertyBag["items"] = items;
		}
	}
}