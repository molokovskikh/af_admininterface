using System;
using AdminInterface.Mailers;
using Castle.Core.Smtp;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using log4net;

namespace AdminInterface.Models.Audit
{
	public class NotifyAttribute : SendEmail
	{
		public NotifyAttribute()
			: base(typeof(ChangeNotificationSender), "RegisterList@subscribe.analit.net")
		{
		}
	}

	public class NotifyBilling : SendEmail
	{
		public NotifyBilling()
			: base(typeof(ChangeNotificationSender), "BillingList@analit.net")
		{
		}
	}

	public class NotifyNews : SendEmail
	{
		public NotifyNews()
			: base(typeof(ChangeNotificationSender), "AFNews@subscribe.analit.net")
		{
		}
	}

	public interface IChangesNotificationAware
	{
		bool ShouldNotify();
	}

	public interface INotificationAware
	{
		string NotifyMessage { get; set; }
	}

	public class ChangeNotificationSender : ISendNoticationChangesInterface
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(ChangeNotificationSender));

		//для тестирования
		public static IEmailSender Sender;
		public static bool UnderTest;

		public void Send(AuditableProperty property, object entity, string to)
		{
			try {
				MonorailMailer mailer;
				if (Sender != null)
					mailer = new MonorailMailer(Sender);
				else
					mailer = new MonorailMailer();
				mailer.UnderTest = UnderTest;

				mailer.NotifyAboutChanges(property, entity, to);
			}
			catch (Exception ex) {
				_log.Error("Ошибка отправки уведомлений об изменении наблюдаемых полей", ex);
			}
		}
	}
}