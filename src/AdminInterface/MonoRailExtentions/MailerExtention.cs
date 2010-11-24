using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.Components.Common.EmailSender;
using Castle.Components.Common.EmailSender.Smtp;
using Castle.MonoRail.Framework;
using Message = Castle.Components.Common.EmailSender.Message;

namespace AdminInterface.MonoRailExtentions
{
	public static class MailerExtention
	{
		public static MonorailMailer Mail(this SmartDispatcherController controller)
		{
			var mailer = new MonorailMailer();
			mailer.Controller = controller;
			return mailer;
		}
	}

	public class BaseMailer
	{
		public bool UnderTest;
		public SmartDispatcherController Controller;
		protected string To;
		protected string From;
		protected string Subject;
		protected bool IsBodyHtml;

		protected string Template;
		protected IDictionary PropertyBag = new Dictionary<string, object>();

		private IEmailSender _sender;

		public BaseMailer(IEmailSender sender)
		{
			_sender = sender;
		}

		public BaseMailer()
		{
			_sender = new SmtpSender(ConfigurationManager.AppSettings["SmtpServer"]);
		}

		public virtual void Send()
		{
			var message = GetMessage();
#if DEBUG
			if (!UnderTest)
				message.To = ConfigurationManager.AppSettings["DebugMail"];
#endif 
			_sender.Send(message);
		}

		protected virtual Message GetMessage()
		{
			var message = Controller.RenderMailMessage(Template, null, PropertyBag);
			message.Subject = Subject;
			message.From = From;
			message.To = To;
			message.Encoding = Encoding.UTF8;
			if (IsBodyHtml)
				message.Format = Format.Html;
			else
				message.Format = Format.Text;
			return message;
		}
	}

	public class MonorailMailer : BaseMailer
	{
		public MonorailMailer(IEmailSender sender) : base(sender)
		{}

		public MonorailMailer()
		{}

		public string Me()
		{
			var request = Controller.Request;
			var result = request.Uri.AbsoluteUri.Replace(request.Uri.AbsolutePath, "") + request.ApplicationPath;
			return result;
		}

		public MonorailMailer EnableChanged(IEnablable item, bool oldEnable)
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
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["Me"] = Me();
		}

		public void NotifySupplierAboutAddressRegistration(Address address)
		{

		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client)
		{

		}
	}
}