﻿using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.Core.Smtp;
using Common.Web.Ui.Helpers;
using ExcelLibrary.SpreadSheet;

namespace AdminInterface.MonoRailExtentions
{
	public class MonorailMailer : BaseMailer
	{
		public MonorailMailer(IEmailSender sender) : base(sender)
		{}

		public MonorailMailer()
		{}

		public static void Deliver(Action<MonorailMailer> domail)
		{
			var mailer = new MonorailMailer();
			domail(mailer);
			mailer.Send();
		}

		public string Me()
		{
			var request = Controller.Request;
			var result = request.Uri.AbsoluteUri.Replace(request.Uri.AbsolutePath, "") + request.ApplicationPath;
			return result;
		}

		public MonorailMailer EnableChanged(IEnablable item)
		{
			Template = "EnableChanged";
			To = "RegisterList@subscribe.analit.net";
			From = "register@analit.net";
			var lastDisable = "неизвестно";

			var type = "";
			if (item is User)
			{
				type = "пользователя";
				var user = ((User) item);
				PropertyBag["client"] = user.Client;
				var disable = UserLogRecord.LastOff(user.Id);
				if (disable != null)
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
			}
			if (item is Address)
			{
				type = "адреса";
				var address = ((Address) item);
				PropertyBag["client"] = address.Client;
				var disable = AddressLogRecord.LastOff(address.Id);
				if (disable != null)
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
			}
			if (item is Client)
			{
				type = "клиента";
				var client = (Client) item;
				PropertyBag["client"] = client;
				var disable = ClientLogRecord.LastOff(client);
				if (disable != null)
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
			}
			if (item.Enabled)
				Subject = String.Format("Возобновлена работа {0}", type);
			else
				Subject = String.Format("Приостановлена работа {0}", type);
			PropertyBag["lastDisable"] = lastDisable;
			PropertyBag["item"] = item;
			PropertyBag["admin"] = SecurityContext.Administrator;
			return this;
		}

		public void FreePasswordChange(User user, string reason)
		{
			To = "RegisterList@subscribe.analit.net";
			From = "register@analit.net";
			Subject = String.Format("Бесплатное изменение пароля - {0}", user.Client.FullName);
		}

		public void PasswordChange(User user)
		{
			To = "billing@analit.net";
			From = "register@analit.net";
			Subject = String.Format("Платное изменение пароля - {0}", user.Client.FullName);
		}

		public void NotifyBillingAboutClientRegistration(Client client)
		{
			Template = "NotifyBillingAboutClientRegistration";
			IsBodyHtml = true;
			To = "billing@analit.net";
			From = "register@analit.net";
			Subject = "Регистрация нового клиента";

			PropertyBag["client"] = client;
			PropertyBag["payer"] = client.Payers.First();
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["Me"] = Me();
		}

		public void NotifySupplierAboutAddressRegistration(Address address)
		{

		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client)
		{

		}

		public void Invoice(Invoice invoice)
		{
			Template = "Invoice";
			Layout = "Print";
			IsBodyHtml = true;

			From = "billing@analit.net";
			To = invoice.Payer.GetInvocesAddress();
			Subject = String.Format("Счет за {0}", BindingHelper.GetDescription(invoice.Period));

			PropertyBag["invoice"] = invoice;
		}

		public void DoNotHaveInvoiceContactGroup(Invoice invoice)
		{
			Template = "DoNotHaveInvoiceContactGroup";

			To = "billing@analit.net";
			From = "billing@analit.net";
			Subject = "Не удалось отправить счет";
			PropertyBag["invoice"] = invoice;
		}

		public MonorailMailer RevisionAct(RevisionAct act, string emails)
		{
			Template = "RevisionAct";

			To = emails;
			From = "billing@analit.net";
			Subject = String.Format("Акт сверки");

			var file = new MemoryStream();
			var book = new Workbook();
			book.Worksheets.Add(Exporter.Export(act));
			book.Save(file);
			file.Position = 0;

			Attachments.Add(new Attachment(file, "Акт сверки.xls"));
			PropertyBag["act"] = act;
			return this;
		}

		public MonorailMailer UserMoved(User user, Client oldClient, Payer oldPayer)
		{
			Template = "UserMoved";

			From = "register@analit.net";
			Subject = "Перемещение пользователя";
			To = "RegisterList@subscribe.analit.net";
			PropertyBag["user"] = user;
			PropertyBag["oldClient"] = oldClient;
			PropertyBag["oldPayer"] = oldPayer;

			return this;
		}

		public MonorailMailer AddressMoved(Address address, Client oldClient, LegalEntity oldLegalEntity)
		{
			Template = "AddressMoved";

			From = "register@analit.net";
			Subject = "Перемещение адреса доставки";
			To = "RegisterList@subscribe.analit.net";
			PropertyBag["address"] = address;
			PropertyBag["oldClient"] = oldClient;
			PropertyBag["oldLegalEntity"] = oldLegalEntity;

			return this;
		}
	}
}