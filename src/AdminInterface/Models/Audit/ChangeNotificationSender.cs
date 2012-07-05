﻿using System;
using AdminInterface.Mailers;
using Common.Web.Ui.Helpers;
using log4net;

namespace AdminInterface.Models.Audit
{
	public class NotifyAttribute : SendEmail
	{
		public NotifyAttribute()
			: base(typeof(ChangeNotificationSender))
		{
		}
	}

	public class ChangeNotificationSender : ISendNoticationChangesInterface
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (ChangeNotificationSender));

		public void Send(AuditableProperty property, object entity)
		{
			try {
				var mailer = new MonorailMailer();
				mailer.NotifyAboutChanges(property, entity);
			}
			catch (Exception ex) {
				_log.Error("Ошибка отправки уведомлений об изменении наблюдаемых полей", ex);
			}
		}
	}
}