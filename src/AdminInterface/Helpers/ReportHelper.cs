using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using AdminInterface.Models;
using AdminInterface.Properties;
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
			DateTime operationDate,
			bool isRegistration)
		{
			var report = new LocalReport
			{
				ReportEmbeddedResource = "AdminInterface.ClientCard.rdlc",
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

			var phones = DefaultValues.Get().Phones;
			phones = "Режим работы: Понедельник – Пятница с 9.00 до 18.00 часов по московскому времени\r\n"
				+ "Телефоны:\r\n"
				+ phones + "\r\n"
				+ "e-mail: tech@analit.net";
			
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

		public static int SendClientCard(User user,
			string password,
			bool isRegistration,
			params string[] mails)
		{
			var defaults = DefaultValues.Get();
			string body;
			var client = user.Client;
			if (client.IsDrugstore())
				body = Settings.Default.RegistrationCardEmailBodyForDrugstore;
			else
				body = Settings.Default.RegistrationCardEmailBodyForSupplier;

			using (var stream = CreateReport(client.Id,
				user.Payer.Id,
				client.Name,
				client.FullName,
				client.GetHumanReadableType(),
				user.Login,
				password,
				DateTime.Now,
				isRegistration))
			using (var message = new MailMessage
			{
				From = new MailAddress("tech@analit.net"),
				Subject = "Регистрационная карта для работы в системе АналитФармация",
				Body = defaults.AppendFooter(body),
				Attachments = { new Attachment(stream, "Регистрационная карта.jpg") },
			})
			{
				foreach (var mail in mails)
					EmailHelper.BuildAttachementFromString(mail, message);

				return Func.Send(message);
			}
		}
	}
}
