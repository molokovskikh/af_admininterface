using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using ConsoleApplication11.ServiceReference2;
using log4net;
using RemotePricePricessor;

namespace AdminInterface.Helpers
{
	public enum ServiceStatus
	{
		[Description("Запущена")] Running,
		[Description("Не запущена")] NotRunning,
		[Description("Не доступена")] Unknown
	}

	public class RemoteServiceHelper
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (RemoteServiceHelper));

		public static void RemotingCall(Action<IRemotePriceProcessor> action)
		{
			try
			{
				var priceProcessor = (IRemotePriceProcessor)Activator.GetObject(typeof(IRemotePriceProcessor), "http://fms.adc.analit.net:888/RemotePriceProcessor");
				action(priceProcessor);
			}
			catch (Exception e)
			{
				_log.Warn("Ошибка при обращении к сервису обработки прайс листов", e);
			}
		}

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

		public static T Try<T>(Func<T> action, Func<Exception, T> fail)
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

		public static void Try(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				_log.Error(e);
			}
		}
	}
}
