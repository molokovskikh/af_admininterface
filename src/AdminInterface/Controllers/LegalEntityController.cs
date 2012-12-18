using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MonoRail.ActiveRecordSupport;
using Common.Tools;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.ViewDrugstore),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class LegalEntityController : AdminInterfaceController
	{
		public void Show(uint id, uint clientId)
		{
			PropertyBag["entity"] = DbSession.Load<LegalEntity>(id);
			PropertyBag["headerCssClass"] = "headerCssClass";
			PropertyBag["bodyCssClass"] = "bodyCssClass";
			PropertyBag["client"] = DbSession.Load<Client>(clientId);
		}

		public void Delete(uint id)
		{
			var addresses = DbSession.QueryOver<Address>().Where(a => a.LegalEntity.Id == id).List();
			if (addresses.Count == 0) {
				var entity = DbSession.Load<LegalEntity>(id);
				DbSession.Delete(entity);
				Notify("Удалено");
			}
			else {
				Error(string.Format("Данное юр. лицо подключено к {1} адресам доставки: <br/> {0}...", addresses.Select(a => a.Name).Take(3).Implode("<br/>"), addresses.Count));
			}
			RedirectToReferrer();
		}

		public void Add(uint clientId)
		{
			PropertyBag["client"] = DbSession.Load<Client>(clientId);
			PropertyBag["JuridicalOrganization"] = new LegalEntity();
		}
	}
}