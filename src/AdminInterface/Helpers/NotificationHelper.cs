using System;
using AddUser;
using AdminInterface.Models;

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

		public static void NotifySupplierAboutDrugstoreRegistration(string clientCode, string fullName, string shortName, string region, string to)
		{
			Func.Mail("pharm@analit.net",
			          "Аналитическая Компания Инфорум",
			          "Новый клиент в системе \"АналитФАРМАЦИЯ\"",
			          false,
			          String.Format(_messageTemplateForSupplierAfterDrugstoreRegistration, 
									fullName, 
									shortName, 
									region, 
									clientCode),
			          to,
					  "",
			          null);
		}

		private const string _messageTemplateForBillingAfterPassordChange = 
@"Добрый день.

Данное сообщение информирует Вас о необходимости начисления стоимости изменения пароля для следующего клиента: 

Наименование: {0} 
Login: {1}
Код: {2}
Договор: {3}

Причина изменения: 
{4}

Сообщение создано автоматической системой изменения паролей {5}";

		private const string _messageTemplateAfterFreePasswordChange =
@"Operator: {0}
Host: {1}
Login: {2}
ID: {3}

Причина: {4}";

		private const string _messageTemplateForAdministratorAfterPasswordChange =
@"Для клиента {0} был успешно изменен пароль";

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
						  String.Empty,
						  String.Format("Бесплатное изменение пароля - {0}", client.FullName),
						  false,
						  String.Format(_messageTemplateAfterFreePasswordChange,
										administrator.UserName, 
										host,
										user.Login,
										client.Id,
										reason),
						  "RegisterList@subscribe.analit.net",
						  String.Empty,
						  null);

			else
				Func.Mail("register@analit.net",
				          String.Empty,
						  String.Format("Платное изменение пароля - {0}", client.FullName),
				          false,
						  String.Format(_messageTemplateForBillingAfterPassordChange,
										client.FullName,
										user.Login,
										client.Id,
										client.BillingInstance.PayerID,
										reason,
										DateTime.Now),
				          "billing@analit.net",
				          String.Empty,
				          "");


			Func.Mail("register@analit.net",
			          String.Empty,
			          String.Format("Успешное изменение пароля - {0}", client.FullName),
			          false,
			          String.Format(_messageTemplateForAdministratorAfterPasswordChange, client.FullName),
			          administrator.Email,
			          String.Empty,
			          null);
		}
	}
}
