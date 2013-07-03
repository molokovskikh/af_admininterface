using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using NHibernate.Criterion;
using NHibernate.Linq;
using ViewHelper = Common.Web.Ui.Helpers.ViewHelper;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.ManageCallbacks),
		Helper(typeof(ViewHelper)),
	]
	public class TelephonyController : AdminInterfaceController
	{
		public TelephonyController()
		{
			SetARDataBinder();
		}

		public void Show()
		{
			PropertyBag["callbacks"] = DbSession.Query<Callback>().OrderBy(c => c.Comment).ToArray();
		}

		public void UpdateCallbacks([ARDataBind("callbacks", AutoLoad = AutoLoadBehavior.Always)] Callback[] callbacks)
		{
			foreach (var callback in callbacks)
				DbSession.Save(callback);

			Flash["isUpdated"] = true;
			RedirectToAction("Show");
		}

		public void Update([DataBind("callback")] Callback callback)
		{
			DbSession.Save(callback);
			Flash["isUpdated"] = true;
			RedirectToAction("Show");
		}

		public void Edit(uint id)
		{
			PropertyBag["callback"] = DbSession.Load<Callback>(id);
		}

		public void New()
		{
			PropertyBag["callback"] = new Callback();
			RenderView("Edit");
		}

		public void Delete(uint id)
		{
			DbSession.Delete(DbSession.Load<Callback>(id));
			RedirectToAction("Show");
		}
	}
}