﻿using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Models.Telephony;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(HttpUtility)),
		Secure(PermissionType.ViewSuppliers),
	]
	public class SuppliersController : AdminInterfaceController
	{
		public void Show(uint id)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			PropertyBag["supplier"] = supplier;
			PropertyBag["users"] = supplier.Users;
			PropertyBag["contactGroups"] = supplier.ContactGroupOwner.ContactGroups;
			PropertyBag["usersInfo"] = ADHelper.GetPartialUsersInformation(supplier.Users);

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["messages"] = ClientInfoLogEntity.MessagesForClient(supplier);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl;

			Sort.Make(this);

			if (IsPost)
			{
				BindObjectInstance(supplier, "supplier");
				if (IsValid(supplier))
				{
					supplier.Save();
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
		}

		public void SendMessage(uint id, string message)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			if (!string.IsNullOrWhiteSpace(message))
			{
				new ClientInfoLogEntity(message, supplier).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}
	}
}
