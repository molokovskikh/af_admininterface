﻿using System;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using System.Web;

namespace AdminInterface.Helpers
{
	public class NotificationHelper
	{
		private const string _messageTemplateForBillingAfterPassordChange =
			@"Добрый день.

Данное сообщение информирует Вас о необходимости начисления стоимости изменения пароля для следующего клиента: 

Наименование: {0} 
Имя пользователя: {1}
Код: {2}
Договор: {3}

Причина изменения: 
{4}

Сообщение создано автоматической системой изменения паролей {5}";

		private const string _messageTemplateAfterFreePasswordChange =
			@"Оператор {0} с хоста {1} изменил пароль пользователя {2}, клиента {3} с кодом {4}
Дата изменения {5}

Причина: {6}";

		public static void NotifyAboutPasswordChange(Administrator administrator,
			User user,
			string password,
			bool isFree,
			string host,
			string reason)
		{
			if (isFree)
				Func.Mail("register@analit.net",
					String.Format("Бесплатное изменение пароля - {0}", user.RootService.FullName),
					String.Format(_messageTemplateAfterFreePasswordChange,
						administrator.UserName,
						host,
						user.Login,
						user.RootService.Name,
						user.RootService.Id,
						DateTime.Now,
						reason),
					"RegisterList@subscribe.analit.net");
			else
				Func.Mail("register@analit.net",
					String.Format("Платное изменение пароля - {0}", user.RootService.FullName),
					String.Format(_messageTemplateForBillingAfterPassordChange,
						user.RootService.FullName,
						user.Login,
						user.RootService.Id,
						user.Payer.Id,
						reason,
						DateTime.Now),
					"billing@analit.net");
		}

		public static void NotifyAboutRegistration(string subject, string body)
		{
			Func.Mail("register@analit.net",
				subject,
				body,
				"RegisterList@subscribe.analit.net",
				SecurityContext.Administrator.Email);
		}
	}
}