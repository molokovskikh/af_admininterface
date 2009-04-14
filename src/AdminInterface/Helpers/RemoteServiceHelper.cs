using System;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using AddUser;
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

		public static ServiceStatus GetServiceStatus(string host, string serviceName)
		{
			return Try(
				() => {
					using (WindowsIdentity.GetCurrent().Impersonate())
					{
						return ServiceController.GetServices(host)
								.First(s => s.ServiceName == serviceName)
								.Status == ServiceControllerStatus.Running
								? ServiceStatus.Running
								: ServiceStatus.NotRunning;
					}
				},
				e => {
					_log.Error(String.Format("Не удалось получить состояние сервиса {0}", serviceName), e);
					return ServiceStatus.Unknown;
				});
		}

		private static T Try<T>(Func<T> action, Func<Exception, T> fail)
		{
			try
			{
				return action();
			}
			catch (Exception e)
			{
				return fail(e);
			}
		}
	}
}
