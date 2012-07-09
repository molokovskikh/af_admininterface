using System;
using AdminInterface.Mailers;
using Castle.Core.Smtp;
using Common.Web.Ui.Helpers;
using log4net;

namespace AdminInterface.Models.Audit
{
	public class NotifyAttribute : SendEmail
	{
		public NotifyAttribute()
			: base(typeof(ChangeNotificationSender))
		{}
	}

	public class NotifyBilling : NotifyAttribute
	{}

	public interface IChangesNotificationAware
	{
		bool ShouldNotify();
	}

	public class ChangeNotificationSender : ISendNoticationChangesInterface
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (ChangeNotificationSender));

		//для тестирования
		public static IEmailSender Sender;
		public static bool UnderTest;

		public void Send(AuditableProperty property, object entity)
		{
			try {
				MonorailMailer mailer;
				if (Sender != null)
					mailer = new MonorailMailer(Sender);
				else
					mailer = new MonorailMailer();
				mailer.UnderTest = UnderTest;

				var to = "RegisterList@subscribe.analit.net";
				var attributes = property.Property.GetCustomAttributes(typeof(NotifyBilling), true);
				if (attributes.Length > 0) {
					to = "BillingList@analit.net";
				}

				mailer.NotifyAboutChanges(property, entity, to);

			}
			catch (Exception ex) {
				Console.WriteLine(ex);
				_log.Error("Ошибка отправки уведомлений об изменении наблюдаемых полей", ex);
			}
		}
	}
}