using System;
using System.Security.Principal;
using System.ServiceModel;
using ConsoleApplication11.ServiceReference2;
using log4net;

namespace AdminInterface.Helpers
{
	public class RemoteServiceHelper
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (RemoteServiceHelper));

		public static void TryDoCall(Action<StatisticServiceClient> action)
		{
			using (WindowsIdentity.GetCurrent().Impersonate())
			{
				StatisticServiceClient client = null;
				try
				{
					client = new StatisticServiceClient();
					client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
					client.ClientCredentials.Windows.AllowNtlm = true;
					action(client);
					client.Close();
				}
				catch (Exception e)
				{
					if (client != null && client.State != CommunicationState.Closed)
						client.Abort();

					_log.Warn("Ошибка при обращении к сервису подготовки данных", e);
				}
			}
		}
	}
}
