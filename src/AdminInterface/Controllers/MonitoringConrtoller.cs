using System;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.PrgData;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Controller = AdminInterface.MonoRailExtentions.Controller;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof (ViewHelper)),
		Helper(typeof (BindingHelper)),
		Layout("GeneralWithJQueryOnly"),
		Filter(ExecuteWhen.BeforeAction, typeof (SecurityActivationFilter))
	]
	public class MonitoringController : Controller
	{
		public void Updates(string sortBy, string direction)
		{
			UpdatingClientStatus[] info;
			//сраная магия, хрен его знает почему и как это работает
			using (WindowsIdentity.GetCurrent().Impersonate())
			{
				StatisticServiceClient client = null;
				try
				{
					client = new StatisticServiceClient();
					client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
					client.ClientCredentials.Windows.AllowNtlm = true;
					info = client.GetUpdateInfo();
					client.Close();
				}
				catch (Exception)
				{
					if (client.State != CommunicationState.Closed)
						client.Abort();
					throw;
				}
			}

			foreach (var status in info)
				status.FetchClient();

			info = info.Where(i => i.Client != null).ToArray();

			PropertyBag["statuses"] = info.Sort(ref sortBy, ref direction, "StartTime").ToArray();
			PropertyBag["sortBy"] = sortBy;
			PropertyBag["direction"] = direction;
		}

		public void Prices()
		{
		}

		public void Orders()
		{
			PropertyBag["Orders"] = OrderView.FindNotSendedOrders();
		}

		[RequiredPermission(PermissionType.MonitorUpdates, PermissionType.ViewDrugstore)]
		public void Clients()
		{
			Clients(4, 1, 8, 0, null, null);
		}

		[RequiredPermission(PermissionType.MonitorUpdates, PermissionType.ViewDrugstore)]
		public void Clients(uint filter, uint type, uint days, ulong region, string sort, string direction)
		{
			if (days < 1)
				days = 1;
			if (days > 40)
				days = 40;

			PropertyBag["filter"] = filter;
			PropertyBag["days"] = days;
			PropertyBag["type"] = type;
			PropertyBag["region"] = region;
			PropertyBag["sort"] = sort;
			PropertyBag["direction"] = direction;
			PropertyBag["regions"] = Region.FindAll();

			if (filter == 4)
				PropertyBag["logEntities"] = ClientRegistrationLogEntity.NotUpdated(days, type, region, sort, direction);
			else if (filter == 5)
				PropertyBag["logEntities"] = ClientRegistrationLogEntity.NotOrdered(days, type, region, sort, direction);
		}
	}
}
