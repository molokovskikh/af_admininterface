using System;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;

namespace AdminInterface.Helpers
{
	public class NotificationHelper
	{
		private static readonly string _messageTemplateForSupplierAfterDrugstoreRegistration = 
@"Добрый день. 

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, зарегистрирован новый клиент: {0} ( {1} ) в регионе(городе) {2}.
Пожалуйста произведите настройки для данного клиента (Раздел 'Для зарегистрированных пользователей' на сайте www.analit.net ).

Адрес доставки накладных: {3}@waybills.analit.net

С уважением, Аналитическая компания 'Инфорум', г. Воронеж

Москва +7 495 6628727
С.-Петербург +7 812 3090521
Воронеж +7 4732 606000
Челябинск +7 351 7298143
".Replace('\'', '\"');

		public static void NotifySupplierAboutDrugstoreRegistration(Client client, string to)
		{
			Func.Mail("tech@analit.net",
				"Аналитическая Компания Инфорум",
				"Новый клиент в системе \"АналитФАРМАЦИЯ\"",
				String.Format(_messageTemplateForSupplierAfterDrugstoreRegistration,
					client.FullName,
					client.ShortName,
					client.HomeRegion.Name,
					client.Id),
				to,
				"",
				null);
		}

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

		public static void NotifyAboutPasswordChange(Client client, 
													 Administrator administrator, 
													 User user,
													 string password,
													 bool isFree, 
													 string host, 
													 string reason)
		{
			if (isFree)
				Func.Mail("register@analit.net",
					String.Format("Бесплатное изменение пароля - {0}", client.FullName),
					String.Format(_messageTemplateAfterFreePasswordChange,
						administrator.UserName,
						host,
						user.Login,
						client.ShortName,
						client.Id,
						DateTime.Now,
						reason),
					"RegisterList@subscribe.analit.net");

			else
				Func.Mail("register@analit.net",
					String.Format("Платное изменение пароля - {0}", client.FullName),
					String.Format(_messageTemplateForBillingAfterPassordChange,
						client.FullName,
						user.Login,
						client.Id,
						client.BillingInstance.PayerID,
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
