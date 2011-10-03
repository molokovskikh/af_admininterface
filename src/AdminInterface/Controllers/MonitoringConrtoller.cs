﻿using System;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.PrgData;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof (ViewHelper)),
		Helper(typeof (BindingHelper)),
		Filter(ExecuteWhen.BeforeAction, typeof (SecurityActivationFilter))
	]
	public class MonitoringController : AdminInterfaceController
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

		public void Orders()
		{
			PropertyBag["Orders"] = OrderView.FindNotSendedOrders();
		}
	}
}
