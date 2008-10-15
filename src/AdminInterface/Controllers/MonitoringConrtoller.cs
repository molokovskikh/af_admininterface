using System;
using System.Security.Principal;
using System.ServiceModel;
using AdminInterface.Helpers;
using AdminInterface.Models.PrgData;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using ConsoleApplication11.ServiceReference2;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof (ViewHelper)),
		Layout("Logs")
	]
	public class MonitoringController : SmartDispatcherController
	{
		public void UpdatingClients()
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

			PropertyBag["statuses"] = info;

		}
	}
}
