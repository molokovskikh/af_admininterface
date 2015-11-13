using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using AddUser;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Properties;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Microsoft.Reporting.WinForms;

namespace AdminInterface.Helpers
{
	public class ReportHelper
	{
		public static Stream CreateReport(uint clientCode,
			uint billingCode,
			string clientShortName,
			string clientFullName,
			string clientType,
			string clientLogin,
			string clientPassword,
			string addresses,
			DateTime operationDate,
			bool isRegistration,
			DefaultValues defaults)
		{
			var resource = "AdminInterface.ClientCard.rdlc";
			if (addresses != null)
				resource = "AdminInterface.ClientCardDrugstore.rdlc";
			var report = new LocalReport {
				ReportEmbeddedResource = resource,
			};
			var deviceInfo =
				@"<DeviceInfo>
	<DpiX>150</DpiX>
	<DpiY>150</DpiY>
    <OutputFormat>JPEG</OutputFormat>
    <MarginTop>0.25in</MarginTop>
    <MarginLeft>0.25in</MarginLeft>
    <MarginRight>0.25in</MarginRight>
    <MarginBottom>0.25in</MarginBottom>
  </DeviceInfo>";
			Warning[] warnings;

			var phones = defaults.Phones;
			phones = "Режим работы: Понедельник – Пятница с 9.00 до 18.00 часов по московскому времени\r\n" + "Телефоны:\r\n" + phones + "\r\n" + "e-mail: tech@analit.net";

			report.SetParameters(new List<ReportParameter> {
				new ReportParameter("ClientCode", clientCode.ToString()),
				new ReportParameter("BillingCode", billingCode.ToString()),
				new ReportParameter("ClientFullName", clientFullName),
				new ReportParameter("ClientShortName", clientShortName),
				new ReportParameter("ClientType", clientType),
				new ReportParameter("ClientLogin", clientLogin),
				new ReportParameter("ClientPassword", clientPassword),
				new ReportParameter("OperationDate", operationDate.ToString()),
				new ReportParameter("IsRegistration", isRegistration.ToString()),
				new ReportParameter("Addresses", addresses ?? ""),
				new ReportParameter("Phones", phones)
			});

			var stream = new MemoryStream();
			report.Render("Image",
				deviceInfo,
				(name, extention, encoding, mimeType, willSick) => stream,
				out warnings);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public static string SendSms(string message, string[] phonesForSend)
		{
			if (phonesForSend == null || phonesForSend.Length == 0) {
				return "don't point numbers";
			}
			var l = new List<string>();
			foreach (var phone in phonesForSend) {
				if (phone.Length != 10 || !phone.All(char.IsDigit)) {
					l.Add(String.Format("bad format {0}", phone));
					continue;
				}
				// 3517983153 -> 73517983153
				int smsId = Func.SendSms(message, "7" + phone);
				if (smsId == 0) {
#if DEBUG
					l.Add(String.Format("don't sent {0}, debug mode", phone));
#else
					l.Add(String.Format("don't sent {0}, read SMS-serv. log", phone));
#endif
					continue;
				}
				l.Add(String.Format("OK {0}, smsId={1}", phone, smsId));
			}
			return String.Join("; ", l);
		}

		public static string SendSmsPasswordToUser(string login, string password, string[] phonesForSend)
		{
			var message = String.Format("Ваш логин от analit: {0}, ваш пароль: {1}", login, password);
			var response = ReportHelper.SendSms(message, phonesForSend);
			return String.Format("To user: {0}", response);
		}

		public static string SendSmsToRegionalAdmin(string login, string password, string[] phonesForSend)
		{
			var message = String.Format("Пользователь analit с логином {0} получил пароль {1}", login, password);
			string response = ReportHelper.SendSms(message, phonesForSend);
			return String.Format("To admin: {0}", response);
		}


		public static int SendClientCard(User user,
			string password,
			bool isRegistration,
			DefaultValues defaults,
			params string[] mails)
		{
			string body;
			string addresses = null;
			if (user.RootService is Client) {
				body = Settings.Default.RegistrationCardEmailBodyForDrugstore;
				var sb = new StringBuilder();
				var i = 0;
				var limit = 22; //Количество строк в окне адресов
				var availibaleAddresses = user.AvaliableAddresses.OrderBy(a => a.Id).ToList();
				foreach (var address in availibaleAddresses) {
					i++;
					sb.AppendFormat("{0}\n", address.Name);
					//Отображаем только limit адресов, так как остальные просто не влезут
					if (user.AvaliableAddresses.Count > limit && i >= limit - 2) {
						sb.AppendLine("и другие.");
						sb.AppendLine("Полную информацию о доступных адресах можно получить, обратившись в техническую поддержку.");
						break;
					}
				}
				sb.Append("  ");
				addresses = sb.ToString();
			}
			else
				body = Settings.Default.RegistrationCardEmailBodyForSupplier;

			using (var stream = CreateReport(user.RootService.Id,
				user.Payer.Id,
				user.RootService.Name,
				user.RootService.FullName,
				user.RootService.GetHumanReadableType(),
				user.Login,
				password,
				addresses,
				DateTime.Now,
				isRegistration,
				defaults))
			using (var message = new MailMessage {
				From = new MailAddress("tech@analit.net"),
				Subject = "Регистрационная карта для работы в системе АналитФармация",
				Body = defaults.AppendFooter(body),
				Attachments = { new Attachment(stream, "Регистрационная карта.jpg") },
			}) {
				if (user.RootService.IsClient())
					message.Attachments.Add(new Attachment(Path.Combine(Global.Config.DocsPath, "Инструкция по установке.doc")));
				foreach (var mail in mails)
					EmailHelper.BuildAttachementFromString(mail, message);
				if (message.To.Count == 0)
					return 0;

				return Func.Send(message);
			}
		}
	}
}