using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using AdminInterface.Models;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using System.Net.Mail;

namespace AdminInterface.MonoRailExtentions
{
	public static class MailerExtention
	{
		public static IEmailSender SenderForTest;

		public static MonorailMailer Mailer(this SmartDispatcherController controller)
		{
			var mailer = new MonorailMailer();
			if (SenderForTest != null)
				mailer = new MonorailMailer(SenderForTest);
			if (controller.Request != null && controller.Request.Uri != null)
			{
				var request = controller.Request;
				mailer.SiteRoot = request.Uri.AbsoluteUri.Replace(request.Uri.AbsolutePath, "") + controller.Context.ApplicationPath;
			}
			return mailer;
		}
	}

	public class BaseMailer
	{
		public bool UnderTest;
		protected string To;
		protected string From;
		protected string Subject;
		protected bool IsBodyHtml;

		protected string Layout;
		protected string Template;
		protected List<Attachment> Attachments = new List<Attachment>();
		protected IDictionary<string, object> PropertyBag = new Dictionary<string, object>();
		private IEmailSender _sender;

		public static IViewEngineManager ViewEngineManager;

		public BaseMailer(IEmailSender sender)
		{
			_sender = sender;
		}

		public BaseMailer()
		{
			_sender = new DefaultSmtpSender(ConfigurationManager.AppSettings["SmtpServer"]);
		}

		public string SiteRoot { get; set; }

		public virtual void Send()
		{
			if (!HaveMessage)
				return;

			var message = GetMessage();
#if DEBUG
			if (!UnderTest)
			{
				message.To.Clear();
				message.To.Add(ConfigurationManager.AppSettings["DebugMail"]);
			}
#endif
			_sender.Send(message);
		}

		public bool HaveMessage
		{
			get { return Template != null; }
		}

		public void Clear()
		{
			Layout = null;
			Template = null;
			Subject = null;
			From = null;
			To = null;
			IsBodyHtml = false;
			Attachments.Clear();
		}

		protected virtual MailMessage GetMessage()
		{
			var writer = new StringWriter();
			RenderTemplate(Template, writer);

			var message = new MailMessage();
			message.Subject = Subject;
			message.From = new MailAddress(From);
			message.To.Add(To);
			message.BodyEncoding = Encoding.UTF8;
			message.HeadersEncoding = Encoding.UTF8;
			message.IsBodyHtml = IsBodyHtml;
			message.Body = writer.ToString();
			foreach (var attachment in Attachments)
				message.Attachments.Add(attachment);
			return message;
		}

		protected void RenderTemplate(string template, TextWriter writer)
		{
			if (ViewEngineManager == null)
				throw new Exception("Mailer не инициализирован");

			if (!template.StartsWith("/"))
				template = Path.Combine("mail", template);

			PropertyBag["siteroot"] = SiteRoot;
			//хак если вьюха лежит в ресурсах то путь начинающийся / не будет работать, правим это
			if (template.StartsWith("/")) {
				var fixedTemplate = template.Remove(0, 1);
				if (ViewEngineManager.HasTemplate(fixedTemplate))
					template = fixedTemplate;
			}

			ViewEngineManager.Process(template, Layout, writer, PropertyBag);
		}
	}
}