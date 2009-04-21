using System;
using System.Web;
using AdminInterface.Helpers;
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
						HttpContext.Current.Request.UserHostAddress,
			          	client.ShortName,
			          	client.FullName,
			          	client.HomeRegion.Name,
			          	client.GetSubordinateType()),
			          "RegisterList@subscribe.analit.net");
		}
	}
}
