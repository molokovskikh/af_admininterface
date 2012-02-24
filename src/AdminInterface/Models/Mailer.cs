using System;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using AdminInterface.Models.Security;
using AdminInterface.Services;
using log4net;

namespace AdminInterface.Models
{
	public class Mailer
	{
		private static ILog _log = LogManager.GetLogger(typeof(Mailer));

		public static void RegionalAdminCreated(Administrator admin)
		{
			try
			{
				Func.Mail("register@analit.net",
					"Новый сотрудник",
					String.Format(
@"В системе зарегистрирован новый сотрудник
Ф.И.О.: {0}
Телефон: {1}
Email: {2}
Подразделение: {3}
", admin.ManagerName,
						admin.PhoneSupport,
						admin.Email,
						admin.Department.GetDescription()),
					"Help@analit.net");
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void RegionalAdminBlocked(Administrator admin)
		{
			try
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
			catch(Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
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
			try
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
						SecurityContext.Administrator.Host,
						client.Name,
						client.FullName,
						client.HomeRegion.Name),
					"RegisterList@subscribe.analit.net");
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void AddressRegistrationResened(Address address)
		{
			try
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
						SecurityContext.Administrator.Host,
						client.Name,
						client.FullName,
						client.HomeRegion.Name,
						address.Value),
					"RegisterList@subscribe.analit.net");
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		private static void SupplierRegistred(string shortname, string homeregion)
		{
			try
			{
				Func.Mail("register@analit.net",
					"Зарегистрирован новый поставщик",
					String.Format(
	@"Краткое наименование: {0}
Домашний регион: {1}", shortname, homeregion),
					"farm@analit.net");
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void Registred(object item, string billingMessage)
		{
			try
			{
				Client client;
				string body;
				string subject;
				Account account;
				if (!String.IsNullOrWhiteSpace(billingMessage))
					billingMessage = "Сообщение в биллинг: " + billingMessage;

				if (item is User)
				{
					var user = ((User)item);
					account = user.Accounting;
					body = "Зарегистрирован новый пользователь " + user.Login;
					subject = "Регистрация нового пользователя";
					client = user.Client;

					if (!String.IsNullOrEmpty(billingMessage))
						new ClientInfoLogEntity(billingMessage, item).Save();
				}
				else
				{
					var address = ((Address)item);
					account = address.Accounting;
					body = "Зарегистрирован новый адрес доставки " + address.Value;
					subject = "Регистрация нового адреса доставки";
					client = address.Client;

					if (!String.IsNullOrEmpty(billingMessage))
						foreach (var user in address.AvaliableForUsers)
							new ClientInfoLogEntity(billingMessage, user).Save();
				}

				if (!String.IsNullOrEmpty(billingMessage))
				{
					body += "\r\n" + billingMessage;
				}

				if (account.IsFree)
					body += "\r\n" + account.RegistrationMessage;

				Func.Mail("register@analit.net",
					subject,
					String.Format(@"Для клиента {0} код {1} регион {2}
{3}
Регистратор {4}",
					client.Name,
					client.Id,
					client.HomeRegion.Name,
					body,
					SecurityContext.Administrator.ManagerName),
					"RegisterList@subscribe.analit.net, billing@analit.net");


				if (item is Address)
				{
					NotifySupplierAboutAddressRegistration((Address)item);
				}
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void SendMessageFromBillingToClient(User user, string text, string subject)
		{
			try
			{
				Func.Mail("billing@analit.net", subject, text, user.GetEmailForBilling());
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void NotifySupplierAboutAddressRegistration(Address address)
		{
			try
			{
				new NotificationService().NotifySupplierAboutAddressRegistration(address);
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void SupplierRegistred(Supplier supplier, string billingMessage)
		{
			try
			{
				SupplierRegistred(supplier.Name, supplier.HomeRegion.Name);
				var body = String.Format(
					"Оператор: {0}\nРегион: {1}\nИмя пользователя: {2}\nКод: {3}\nТип: {4}",
					SecurityContext.Administrator.UserName,
					supplier.HomeRegion.Name,
					supplier.Users.First().Login,
					supplier.Id,
					supplier.Type.GetDescription());

				if (!String.IsNullOrEmpty(billingMessage))
					body += "\r\nСообщение в биллинг: " + billingMessage;

				if (supplier.Account.IsFree)
					body += "\r\n" + supplier.Account.RegistrationMessage;

				NotificationHelper.NotifyAboutRegistration(
					String.Format("\"{0}\" - успешная регистрация", supplier.FullName),
					body);
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}

		public static void ClientRegistred(Client client, string billingMessage)
		{
			try
			{
				new NotificationService().NotifySupplierAboutDrugstoreRegistration(client, false);
				var user = client.Users.First();
				var body = String.Format(
					"Оператор: {0}\nРегион: {1}\nИмя пользователя: {2}\nКод: {3}\nТип: {4}",
					SecurityContext.Administrator.UserName,
					client.HomeRegion.Name,
					user.Login,
					client.Id,
					client.Type.GetDescription());

				if (!String.IsNullOrEmpty(billingMessage))
					body += "\r\nСообщение в биллинг: " + billingMessage;

				if (user.Accounting.IsFree)
					body += "\r\n" + user.Accounting.RegistrationMessage;

				NotificationHelper.NotifyAboutRegistration(
					String.Format("\"{0}\" - успешная регистрация", client.FullName),
					body);
			}
			catch (Exception e)
			{
				_log.Error("Ошибка при отправке уведомления", e);
			}
		}
	}
}
