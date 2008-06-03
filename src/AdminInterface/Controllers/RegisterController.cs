using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("billing"), Helper(typeof(BindingHelper)), Helper(typeof(ViewHelper))]
	[Security(PermissionType.RegisterDrugstore, PermissionType.RegisterSupplier, Required = Required.AnyOf)]
	public class RegisterController : SmartDispatcherController
	{
		public void Register(uint id, bool showRegistrationCard)
		{
			var instance = Payer.Find(id);
			PropertyBag["Instance"] = instance;
			PropertyBag["showRegistrationCard"] = showRegistrationCard;
		}

		public void Registered([ARDataBind("Instance", AutoLoadBehavior.Always)] Payer payer,
		                       bool showRegistrationCard)
		{
			payer.UpdateAndFlush();
			if (showRegistrationCard)
				RedirectToUrl("../report.aspx");
			else
				RedirectToAction("SuccessRegistration");
		}

		public void SuccessRegistration()
		{

		}
	}
}
