using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Text;
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

		protected string Layout;
		protected string Template;
		protected IDictionary<string, object> PropertyBag = new Dictionary<string, object>();

		private IEmailSender _sender;

		public static IViewEngineManager ViewEngineManager;

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
			if (ViewEngineManager == null)
				throw new Exception("Mailer не инициализирован");

			if (!Template.StartsWith("/"))
				Template = Path.Combine("mail", Template);

			var writer = new StringWriter();
			ViewEngineManager.Process(Template, Layout, writer, PropertyBag);

			var message = new Message();
			message.Subject = Subject;
			message.From = From;
			message.To = To;
			message.Encoding = Encoding.UTF8;
			if (IsBodyHtml)
				message.Format = Format.Html;
			else
				message.Format = Format.Text;
			message.Body = writer.ToString();
			return message;
		}
	}
}