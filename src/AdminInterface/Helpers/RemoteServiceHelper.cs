using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using RemoteOrderSenderService;
using log4net;
using RemotePriceProcessor;
using AdminInterface.Properties;

namespace AdminInterface.Helpers
{
	public enum ServiceStatus
	{
		[Description("Запущена")] Running,
		[Description("Не запущена")] NotRunning,
		[Description("Недоступна")] Unknown
	}

	public class RemoteServiceHelper
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(RemoteServiceHelper));

		public static void RemotingCall(Action<IRemotePriceProcessor> action)
		{
			var binding = new NetTcpBinding();
			binding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
			binding.Security.Mode = SecurityMode.None;
			binding.TransferMode = TransferMode.Streamed;
			binding.MaxReceivedMessageSize = Int32.MaxValue;
			binding.MaxBufferSize = 524288;

			var channelFactory = new ChannelFactory<IRemotePriceProcessor>(binding, Settings.Default.WCFServiceUrl);
			IRemotePriceProcessor channel = null;
			try {
				channel = channelFactory.CreateChannel();
				action(channel);
				((ICommunicationObject)channel).Close();
			}
			catch (Exception e) {
				_log.Warn("Ошибка при обращении к сервису обработки прайс листов", e);
			}
			finally {
				var communicationObject = (ICommunicationObject)channel;
				if (communicationObject != null
					&& communicationObject.State != CommunicationState.Closed)
					communicationObject.Abort();
				channelFactory.Close();
			}
		}

		public static ServiceStatus GetServiceStatus(string host, string serviceName)
		{
			if (String.IsNullOrEmpty(host) || String.IsNullOrEmpty(serviceName))
				return ServiceStatus.Unknown;
			return Try(
				() => {
					using (WindowsIdentity.GetCurrent().Impersonate()) {
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
			try {
				return action();
			}
			catch (Exception e) {
				return fail(e);
			}
		}

		public static void Try(Action action)
		{
			try {
				action();
			}
			catch (Exception e) {
				_log.Error(e);
			}
		}
	}
}