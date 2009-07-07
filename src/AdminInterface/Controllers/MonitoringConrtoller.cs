using System;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.PrgData;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using ConsoleApplication11.ServiceReference2;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof (ViewHelper)),
		Layout("General")
	]
	public class MonitoringController : SmartDispatcherController
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
	}
}
