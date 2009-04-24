using System;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Security;

namespace AdminInterface.Models
{
	public class Mailer
	{
		public static void ClientRegistrationResened(Client client)
		{
			Func.Mail("register@analit.net",
			          "Разослано повторное уведомление о регистрации",
			          String.Format(
@"Оператор: {0}
Хост: {1}
Краткое наименование: {2}
Полное наименование: {3}
Домашний регион: {4}
Тип подчинения: {5}",
						SecurityContext.Administrator.UserName,
						SecurityContext.Administrator.GetHost(),
			          	client.ShortName,
			          	client.FullName,
			          	client.HomeRegion.Name,
			          	client.GetSubordinateType()),
			          "RegisterList@subscribe.analit.net");
		}

		public static void SupplierRegistred(string shortname, string homeregion)
		{
			Func.Mail("register@analit.net",
				"Зарегистрирован новый поставщик",
				String.Format(
@"Краткое наименование: {0}
Домашний регион: {1}", shortname, homeregion),
				"farm@analit.net");
		}

		public static void ClientBackToWork(Client client)
		{
			var off = ClientLogRecord.LastOff(client.Id);
			var offLetter = "неизвестно";
			if (off != null)
				offLetter = String.Format("{0} пользователем {1}", off.LogTime, off.OperatorName);
			Func.Mail("register@analit.net", 
				"Возобновлена работа клиента", 
				String.Format(
@"Оператор: {0}
Хост: {1}
Краткое наименование: {2}
Полное наименование: {3}
Домашний регион: {4}
Последнее отключение: {5}
", 
					 SecurityContext.Administrator.UserName, 
					 SecurityContext.Administrator.GetHost(),
					 client.ShortName,
					 client.FullName,
					 client.HomeRegion.Name,
					 offLetter),
				"RegisterList@subscribe.analit.net");
		}
	}
}
