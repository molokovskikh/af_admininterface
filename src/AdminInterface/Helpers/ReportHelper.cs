﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using AddUser;
using AdminInterface.Properties;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
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
				ReportEmbeddedResource = "AdminInterface.Registration.rdlc",
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

			report.SetParameters(new List<ReportParameter>
			                     	{
			                     		new ReportParameter("ClientCode", clientCode.ToString()),
										new ReportParameter("BillingCode", billingCode.ToString()),
										new ReportParameter("ClientFullName", clientFullName),
										new ReportParameter("ClientShortName", clientShortName),
										new ReportParameter("ClientType", clientType),
										new ReportParameter("ClientLogin", clientLogin),
										new ReportParameter("ClientPassword", clientPassword),
										new ReportParameter("OperationDate", operationDate.ToString()),
										new ReportParameter("IsRegistration", isRegistration.ToString())
			                     	});

			var stream = new MemoryStream();
			report.Render("Image",
						  deviceInfo,
						  (name, extention, encoding, mimeType, willSick) => stream,
						  out warnings);
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public static void SendRegistrationCard(Client client, User user, string password, string additionTo, bool isRegistration)
		{
			
			using (var stream = CreateReport(client.Id,
											 client.BillingInstance.PayerID,
											 client.ShortName,
											 client.FullName,
											 BindingHelper.GetDescription(client.Type),
											 user.Login,
											 password,
											 DateTime.Now,
											 isRegistration))
			using (var message = new MailMessage
			{
				From = new MailAddress("tech@analit.net"),
				Subject = "Регистрационная карта для работы в системе АналитФармация",
				Body = Settings.Default.RegistrationCardEmailBody,
				Attachments = { new Attachment(stream, "Регистрационная катра.jpg") },
			})
			{
				EmailHelper.BuildAttachementFromString(additionTo, message);
				EmailHelper.BuildAttachementFromString(client.GetPasswordChangeNotificationAddress(), message);
				Func.Send(message);
			}
		}
	}
}
