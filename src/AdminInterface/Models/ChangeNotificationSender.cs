using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using AdminInterface.Models.Suppliers;
using Common.Tools;
using Common.Web.Ui.Helpers;
using log4net;

namespace AdminInterface.Models
{
	public class ChangeNotificationSender : ISendNoticationChangesInterface
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (ChangeNotificationSender));

		public void Send(PropertyInfo property, string name, object newValue, object oldValue, object entity)
		{
			try {
				if (entity is Service)
					NameFullNameChanges(property, name, newValue, oldValue, entity);
			}
			catch (Exception ex) {
				_log.Error("Ошибка отправки уведомлений об изменении наблюдаемых полей", ex);
			}
		}

		public static void NameFullNameChanges(PropertyInfo property, string name, object newValue, object oldValue, object _entity)
		{
			if (String.IsNullOrEmpty(name))
				name = BindingHelper.GetDescription(property);
			var mailer = new MonorailMailer();
			var message = new StringBuilder();
			var clientId = ((Service)_entity).Id;
			message.AppendLine("Клиент " + clientId);
			var entity = _entity as Client;
			if (entity != null) {
				message.AppendLine("Плательщики: " + (entity).Payers.Implode(p => p.Name));
			}
			var supplier = _entity as Supplier;
			if (supplier != null) {
				message.AppendLine("Плательщик: " + (supplier).Payer.Name);
			}
			message.AppendLine(String.Format("Изменено '{0}' было '{1}' стало '{2}'", name, oldValue, newValue));
			mailer.ChangeNameFullName(message.ToString()).Send();
		}
	}
}