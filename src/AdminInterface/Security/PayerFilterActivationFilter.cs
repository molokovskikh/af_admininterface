using System;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Security
{
	public class PayerFilterActivationFilter : IFilter
	{
		public bool Perform(ExecuteWhen exec, 
							IEngineContext context, 
							IController controller,
		                    IControllerContext controllerContext)
		{
			var holder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = holder.CreateSession(typeof (Payer));
			try
			{
				var regionMask = SecurityContext.Administrator.RegionMask;
				session.EnableFilter("HomeRegionFilter").SetParameter("AdminRegionMask", regionMask);
				if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore)
					&& SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
					return true;

				if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore))
					session.EnableFilter("DrugstoreOnlyFilter");

				if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
					session.EnableFilter("SupplierOnlyFilter");
			}
			catch (Exception)
			{
				holder.FailSession(session);
				throw;
			}
			finally
			{
				holder.ReleaseSession(session);
			}
			return true;
		}
	}
}
