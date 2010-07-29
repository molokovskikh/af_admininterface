using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using NHibernate.Criterion;
using ViewHelper=AdminInterface.Helpers.ViewHelper;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.ManageCallbacks),
		Helper(typeof(ViewHelper)),
		Layout("GeneralWithJQuery"),
	]
	public class TelephonyController : ARSmartDispatcherController
	{
		public void Show()
		{
			PropertyBag["callbacks"] = Callback.FindAll(Order.Asc("Comment"));
		}

		public void UpdateCallbacks([ARDataBind("callbacks", AutoLoad = AutoLoadBehavior.Always)] Callback[] callbacks)
		{
			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				foreach (var callback in callbacks)
					callback.Save();
			}

			Flash["isUpdated"] = true;
			RedirectToAction("Show");
		}

		public void Update([DataBind("callback")] Callback callback)
		{
			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				callback.Save();
			}
			
			Flash["isUpdated"] = true;
			RedirectToAction("Show");
		}

		public void Edit(uint id)
		{
			PropertyBag["callback"] = Callback.Find(id);
		}

		public void New()
		{
			PropertyBag["callback"] = new Callback();
			RenderView("Edit");
		}

		public void Delete(uint id)
		{
			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				Callback.Find(id).Delete();
			}
			RedirectToAction("Show");
		}
	}
}
