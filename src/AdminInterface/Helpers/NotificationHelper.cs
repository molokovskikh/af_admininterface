using System;
using System.Text;
using AddUser;

namespace AdminInterface.Helpers
{
	public class NotificationHelper
	{
		private static readonly string _template = 
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
			          String.Format(_template, fullName, shortName, region, clientCode),
			          to,
					  "",
			          null,
			          Encoding.UTF8);
		}
	}
}
