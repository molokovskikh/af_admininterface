using System;
using System.IO;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using AdminInterface.Services;
using log4net;

namespace AdminInterface.Mailers
{
	public class Mailer
	{
		private static ILog _log = LogManager.GetLogger(typeof(Mailer));

		public static void RegionalAdminCreated(Administrator admin)
		{
			try
			{
				Func.Mail("register@analit.net",
					"����� ���������",
					String.Format(
@"� ������� ��������������� ����� ���������
�.�.�.: {0}
�������: {1}
Email: {2}
�������������: {3}
", admin.ManagerName,
						admin.PhoneSupport,
						admin.Email,
						admin.Department.GetDescription()),
					"RegisterList@subscribe.analit.net");
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void RegionalAdminBlocked(Administrator admin)
		{
			try
			{
				Func.Mail("register@analit.net",
					String.Format("������ ������ ��� {0}, {1}", admin.ManagerName, admin.Department.GetDescription()),
					String.Format(
@"� ������� ��������� ������ ����������
�.�.�.: {0}
�������������: {1}
",					admin.ManagerName,
						admin.Department.GetDescription()),
					"RegisterList@subscribe.analit.net");
			}
			catch(Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void RegionalAdminUnblocked(Administrator admin)
		{
			Func.Mail("register@analit.net",
				String.Format("������������� ������ ��� {0}, {1}", admin.ManagerName, admin.Department.GetDescription()),
				String.Format(
@"� ������� ������������ ������ ����������
�.�.�.: {0}
�������������: {1}
",					admin.ManagerName,
					admin.Department.GetDescription()),
				"RegisterList@subscribe.analit.net");
		}

		public static void ClientRegistrationResened(Client client)
		{
			try
			{
				Func.Mail("register@analit.net",
					"��������� ��������� ����������� � �����������",
					String.Format(
@"��������: {0}
����: {1}
������� ������������: {2}
������ ������������: {3}
�������� ������: {4}",
						SecurityContext.Administrator.UserName,
						SecurityContext.Administrator.Host,
						client.Name,
						client.FullName,
						client.HomeRegion.Name),
					"RegisterList@subscribe.analit.net");
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void AddressRegistrationResened(Address address)
		{
			try
			{
				var client = address.Client;
				Func.Mail("register@analit.net",
					"��������� ��������� ����������� � ����������� ������",
					String.Format(
@"��������: {0}
����: {1}
������� ������������: {2}
������ ������������: {3}
�������� ������: {4}
����� ��������: {5}",
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
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		private static void SupplierRegistred(string shortname, string homeregion)
		{
			try
			{
				Func.Mail("register@analit.net",
					"��������������� ����� ���������",
					String.Format(
	@"������� ������������: {0}
�������� ������: {1}", shortname, homeregion),
					"farm@analit.net");
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void Registred(object item, string billingMessage, DefaultValues defaults)
		{
			try
			{
				Client client;
				string body;
				string subject;
				Account account;
				if (!String.IsNullOrWhiteSpace(billingMessage))
					billingMessage = "��������� � �������: " + billingMessage;

				if (item is User)
				{
					var user = ((User)item);
					account = user.Accounting;
					var comment = "�����������: " + user.Name;
					body = "��������������� ����� ������������ \r\n" + user.Login;
					subject = "����������� ������ ������������";
					client = user.Client;
					if (!String.IsNullOrWhiteSpace(billingMessage))
						billingMessage += "\r\n";
					billingMessage += comment;
					if (!String.IsNullOrEmpty(billingMessage))
						new AuditRecord(billingMessage, item).Save();
				}
				else
				{
					var address = ((Address)item);
					account = address.Accounting;
					body = "��������������� ����� ����� �������� " + address.Value;
					subject = "����������� ������ ������ ��������";
					client = address.Client;

					if (!String.IsNullOrEmpty(billingMessage))
						foreach (var user in address.AvaliableForUsers)
							new AuditRecord(billingMessage, user).Save();
				}

				if (!String.IsNullOrEmpty(billingMessage))
				{
					body += "\r\n" + billingMessage;
				}

				if (account.IsFree)
					body += "\r\n" + account.RegistrationMessage;

				Func.Mail("register@analit.net",
					subject,
					String.Format(@"��� ������� {0} ��� {1} ������ {2}
{3}
����������� {4}",
					client.Name,
					client.Id,
					client.HomeRegion.Name,
					body,
					SecurityContext.Administrator.ManagerName),
					"RegisterList@subscribe.analit.net, billing@analit.net");


				if (item is Address)
				{
					NotifySupplierAboutAddressRegistration((Address)item, defaults);
				}
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void SendMessageFromBillingToClient(UserMessage message)
		{
			try
			{
				Func.Mail("billing@analit.net", message.Subject, message.Message, message.To);
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void NotifySupplierAboutAddressRegistration(Address address, DefaultValues defaults)
		{
			try
			{
				new NotificationService(defaults).NotifySupplierAboutAddressRegistration(address);
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void SupplierRegistred(Supplier supplier, string billingMessage)
		{
			try
			{
				SupplierRegistred(supplier.Name, supplier.HomeRegion.Name);
				var body = String.Format(
					"��������: {0}\n������: {1}\n��� ������������: {2}\n���: {3}\n���: {4}",
					SecurityContext.Administrator.UserName,
					supplier.HomeRegion.Name,
					supplier.Users.First().Login,
					supplier.Id,
					supplier.Type.GetDescription());

				if (!String.IsNullOrEmpty(billingMessage))
					body += "\r\n��������� � �������: " + billingMessage;

				if (supplier.Account.IsFree)
					body += "\r\n" + supplier.Account.RegistrationMessage;

				NotificationHelper.NotifyAboutRegistration(
					String.Format("\"{0}\" - �������� �����������", supplier.FullName),
					body);
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}

		public static void ClientRegistred(Client client, string billingMessage, DefaultValues defaults)
		{
			try
			{
				new NotificationService(defaults).NotifySupplierAboutDrugstoreRegistration(client, false);

				var user = client.Users.FirstOrDefault();
				var body = new StringWriter();
				body.WriteLine("��������: {0}", SecurityContext.Administrator.UserName);
				body.WriteLine("������: {0}", client.HomeRegion.Name);
				if (user != null)
					body.WriteLine("��� ������������: {0}", user.Login);
				body.WriteLine("���: {0}", client.Id);

				if (!String.IsNullOrEmpty(billingMessage))
					body.WriteLine("��������� � �������: " + billingMessage);

				if (user != null && user.Accounting.IsFree)
					body.WriteLine(user.Accounting.RegistrationMessage);

				NotificationHelper.NotifyAboutRegistration(
					String.Format("\"{0}\" - �������� �����������", client.FullName),
					body.ToString());
			}
			catch (Exception e)
			{
				_log.Error("������ ��� �������� �����������", e);
			}
		}
	}
}
