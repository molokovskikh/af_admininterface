using System;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using AdminInterface.Models.Security;
using AdminInterface.Services;

namespace AdminInterface.Models
{
	public class Mailer
	{
		public static void RegionalAdminCreated(Administrator admin)
		{
			Func.Mail("register@analit.net",
				"Новый сотрудник",
				String.Format(
@"В системе зарегистрирован новый сотрудник
Ф.И.О.: {0}
Телефон: {1}
Email: {2}
Подразделение: {3}
",					admin.ManagerName,
					admin.PhoneSupport,
					admin.Email,
					admin.Department.GetDescription()),
				"Help@analit.net");
		}

		public static void RegionalAdminBlocked(Administrator admin)
		{
			Func.Mail("register@analit.net",
				String.Format("Запрет работы для {0}, {1}", admin.ManagerName, admin.Department.GetDescription()),
				String.Format(
@"В системе ЗАПРЕЩЕНА работа сотрудника
Ф.И.О.: {0}
Подразделение: {1}
",					admin.ManagerName,
					admin.Department.GetDescription()),
				"Help@analit.net");
		}

		public static void RegionalAdminUnblocked(Administrator admin)
		{
			Func.Mail("register@analit.net",
				String.Format("Возобновление работы для {0}, {1}", admin.ManagerName, admin.Department.GetDescription()),
				String.Format(
@"В системе ВОЗОБНОВЛЕНА работа сотрудника
Ф.И.О.: {0}
Подразделение: {1}
",					admin.ManagerName,
					admin.Department.GetDescription()),
				"Help@analit.net");
		}

		public static void ClientRegistrationResened(Client client)
		{
			Func.Mail("register@analit.net",
				"Разослано повторное уведомление о регистрации",
				String.Format(
@"Оператор: {0}
Хост: {1}
Краткое наименование: {2}
Полное наименование: {3}
Домашний регион: {4}",
					SecurityContext.Administrator.UserName,
					SecurityContext.Administrator.GetHost(),
					client.Name,
					client.FullName,
					client.HomeRegion.Name),
				"RegisterList@subscribe.analit.net");
		}

		public static void AddressRegistrationResened(Address address)
		{
			var client = address.Client;
			Func.Mail("register@analit.net",
				"Разослано повторное уведомление о регистрации адреса",
				String.Format(
@"Оператор: {0}
Хост: {1}
Краткое наименование: {2}
Полное наименование: {3}
Домашний регион: {4}
Адрес доставки: {5}",
					SecurityContext.Administrator.UserName,
					SecurityContext.Administrator.GetHost(),
					client.Name,
					client.FullName,
					client.HomeRegion.Name,
					address.Value),
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

		public static void AddressRegistred(Address address)
		{
			Func.Mail("register@analit.net", 
				"Регистрация нового адреса доставки", 
				String.Format(@"Для клиента {0} код {1} регион {2}
Зарегистрирован новый адрес доставки {3}
Регистратор {4}",
				address.Client.Name,
				address.Client.Id,
				address.Client.HomeRegion.Name,
				address.Value,
				SecurityContext.Administrator.ManagerName), 
				"RegisterList@subscribe.analit.net, billing@analit.net");

			NotifySupplierAboutAddressRegistration(address);
		}

		public static void UserRegistred(User user)
		{
			Func.Mail("register@analit.net", 
				"Регистрация нового пользователя", 
				String.Format(@"Для клиента {0} код {1}
Зарегистрирован новый пользователь {2}
Регистратор {3}",
				user.Client.Name,
				user.Client.Id,
				user.Login,
				SecurityContext.Administrator.ManagerName),
				"RegisterList@subscribe.analit.net, billing@analit.net");
		}

		public static void SendMessageFromBillingToClient(Client client, string text, string subject)
		{
			Func.Mail("billing@analit.net", subject, text, client.GetEmailsForBilling());
		}

		public static void NotifySupplierAboutAddressRegistration(Address address)
		{
			new NotificationService().NotifySupplierAboutAddressRegistration(address);
		}

		public static void ClientRegistred(Client client, bool renotification)
		{
			if (client.IsDrugstore())
				new NotificationService().NotifySupplierAboutDrugstoreRegistration(client, false);
			else
				SupplierRegistred(client.Name, client.HomeRegion.Name);

			if (renotification)
			{
				ClientRegistrationResened(client);
			}
			else
			{
				NotificationHelper.NotifyAboutRegistration(String.Format("\"{0}\" - успешная регистрация", client.FullName),
					String.Format(
						"Оператор: {0}\nРегион: {1}\nИмя пользователя: {2}\nКод: {3}\n\nСегмент: {4}\nТип: {5}",
						SecurityContext.Administrator.UserName,
						client.HomeRegion.Name,
						client.Users.First().Login,
						client.Id,
						client.Segment.GetDescription(),
						client.Type.GetDescription()));
			}
		}
	}
}
