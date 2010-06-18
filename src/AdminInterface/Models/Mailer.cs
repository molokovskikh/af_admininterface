using System;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using AdminInterface.Models.Security;

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

		public static void AddressRegistrationResened(Client client, string address)
		{
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
					address),
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
Код клиента: {6}
Краткое наименование: {2}
Полное наименование: {3}
Домашний регион: {4}
Последнее отключение: {5}
", 
					 SecurityContext.Administrator.UserName, 
					 SecurityContext.Administrator.GetHost(),
					 client.Name,
					 client.FullName,
					 client.HomeRegion.Name,
					 offLetter,
					 client.Id),
				"RegisterList@subscribe.analit.net");
		}

		public static void UserBackToWork(User user)
		{
			var off = UserLogRecord.LastOff(user.Id);
			var offLetter = "неизвестно";
			if (off != null)
				offLetter = String.Format("{0} пользователем {1}", off.LogTime, off.OperatorName);
			Func.Mail("register@analit.net", "Возобновлена работа пользователя",
				String.Format(
@"Оператор: {0}
Хост: {1}
Код клиента: {2}
Логин: {3}
Комментарий: {4}
Домашний регион: {5}
Последнее отключение: {6}
",
				SecurityContext.Administrator.UserName,
				SecurityContext.Administrator.GetHost(),
				user.Client.Id,
				user.Login,
				user.Name,
				user.Client.HomeRegion.Name,
				offLetter),
				"RegisterList@subscribe.analit.net");
		}

		public static void AddressBackToWork(Address address)
		{
			var off = AddressLogRecord.LastOff(address.Id);
			var offLetter = "неизвестно";
			if (off != null)
				offLetter = String.Format("{0} пользователем {1}", off.LogTime, off.OperatorName);
			Func.Mail("register@analit.net", "Возобновлена работа адреса доставки",
				String.Format(
@"Оператор: {0}
Хост: {1}
Код клиента: {2}
Клиент: {7}
Код адреса: {3}
Адрес: {4}
Домашний регион: {5}
Последнее отключение: {6}
",
				SecurityContext.Administrator.UserName,
				SecurityContext.Administrator.GetHost(),
				address.Client.Id,
				address.Id,
				address.Value,
				address.Client.HomeRegion.Name,
				offLetter,
				address.Client.FullName),
				"RegisterList@subscribe.analit.net");			
		}

		public static void DeliveryAddressRegistred(Address address)
		{
			Func.Mail("register@analit.net", 
				"Регистрация нового адреса доставки", 
				String.Format(@"Для клиента {0} код {1}
Зарегистрирован новый адрес доставки {2}
Регистратор {3}",
				address.Client.Name,
				address.Client.Id,
				address.Value,
				SecurityContext.Administrator.ManagerName), 
				"RegisterList@subscribe.analit.net, billing@analit.net");
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

		public static void SendMessageFromBillingToClient(Client client, string text)
		{
			Func.Mail("billing@analit.net", "Сообщение от службы биллинга АК \"Инфорум\"", text, client.GetEmails());
		}
	}
}
