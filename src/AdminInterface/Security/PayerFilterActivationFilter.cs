using AdminInterface.Models.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Security
{
	public class PayerFilterActivationFilter : IFilter
	{
		public bool Perform(ExecuteWhen exec,
			IEngineContext context,
			IController controller,
			IControllerContext controllerContext)
		{
			ArHelper.WithSession(s => {
				var regionMask = SecurityContext.Administrator.RegionMask;
				s.EnableFilter("RegionFilter").SetParameter("AdminRegionMask", regionMask);

				if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore)
					&& SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
					return;

				if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore))
					s.EnableFilter("DrugstoreOnlyFilter");

				if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
					s.EnableFilter("SupplierOnlyFilter");
			});
			return true;
		}
	}

	public class SecurityActivationFilter : IFilter
	{
		public bool Perform(ExecuteWhen exec,
			IEngineContext context,
			IController controller,
			IControllerContext controllerContext)
		{
			ArHelper.WithSession(s => {
				var regionMask = SecurityContext.Administrator.RegionMask;
				s.EnableFilter("RegionFilter").SetParameter("AdminRegionMask", regionMask);
			});

			return true;
		}
	}
}
