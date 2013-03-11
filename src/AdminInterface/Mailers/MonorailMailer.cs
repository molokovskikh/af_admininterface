using System;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Documents;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Castle.Core.Smtp;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;
using ExcelLibrary.SpreadSheet;
using NHibernate;
using log4net;
using Attachment = System.Net.Mail.Attachment;

namespace AdminInterface.Mailers
{
	public class MonorailMailer : BaseMailer
	{
		private static ILog _log = LogManager.GetLogger(typeof(MonorailMailer));

		public MonorailMailer(IEmailSender sender) : base(sender)
		{
		}

		public MonorailMailer()
		{
		}

		public static void Deliver(Action<MonorailMailer> domail)
		{
			var mailer = new MonorailMailer();
			domail(mailer);
			mailer.Send();
		}

		public MonorailMailer EnableChanged(IEnablable item, string comment = null)
		{
			Template = "EnableChanged";
			To = "RegisterList@subscribe.analit.net";
			From = "register@analit.net";
			var lastDisable = "неизвестно";
			var reasonDisable = "неизвестно";

			var type = "";
			var clazz = NHibernateUtil.GetClass(item);
			if (clazz == typeof(User)) {
				type = "пользователя";
				var user = (User)item;
				PropertyBag["service"] = user.RootService;
				var disable = UserLogRecord.LastOff(user.Id);
				if (disable != null) {
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
					if(!item.Enabled) {
						disable.Comment = comment;
						disable.Save();
					}
					reasonDisable = disable.Comment;
				}
			}
			if (clazz == typeof(Address)) {
				type = "адреса";
				var address = (Address)item;
				PropertyBag["service"] = address.Client;
				var disable = AddressLogRecord.LastOff(address.Id);
				if (disable != null) {
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
					if(!item.Enabled) {
						disable.Comment = comment;
						disable.Save();
					}
					reasonDisable = disable.Comment;
				}
			}
			if (clazz == typeof(Client)) {
				type = "клиента";
				var client = ActiveRecordMediator<Client>.FindByPrimaryKey(((Service)item).Id);
				PropertyBag["service"] = client;
				var disable = ClientLogRecord.LastOff(client);
				if (disable != null) {
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
					if(!item.Enabled) {
						disable.Comment = comment;
						disable.Save();
					}
					reasonDisable = disable.Comment;
				}
			}
			if (clazz == typeof(Supplier)) {
				type = "поставщика";
				PropertyBag["service"] = item;
				var disable = SupplierLog.Queryable.Where(s => s.Supplier == (Supplier)item && s.Disabled != null && s.Disabled == true).OrderByDescending(s => s.LogTime).FirstOrDefault();
				if (disable != null) {
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
					if(!item.Enabled) {
						disable.Comment = comment;
						disable.Save();
					}
					reasonDisable = disable.Comment;
				}
			}

			if (item.Enabled)
				Subject = String.Format("Возобновлена работа {0}", type);
			else
				Subject = String.Format("Приостановлена работа {0}", type);
			if (!string.IsNullOrEmpty(lastDisable))
				PropertyBag["lastDisable"] = lastDisable;
			PropertyBag["item"] = item;
			PropertyBag["admin"] = SecurityContext.Administrator;
			if (!string.IsNullOrEmpty(reasonDisable))
				PropertyBag["reasonDisable"] = reasonDisable;
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

		public MonorailMailer NotifyBillingAboutClientRegistration(Client client)
		{
			Template = "NotifyBillingAboutClientRegistration";
			IsBodyHtml = true;
			To = "billing@analit.net";
			From = "register@analit.net";
			Subject = "Регистрация нового клиента";

			PropertyBag["client"] = client;
			PropertyBag["payer"] = client.Payers.First();
			PropertyBag["user"] = client.Users.FirstOrDefault();
			PropertyBag["admin"] = SecurityContext.Administrator;
			return this;
		}

		public void DoNotHaveInvoiceContactGroup(Invoice invoice)
		{
			Template = "DoNotHaveInvoiceContactGroup";
			IsBodyHtml = true;

			To = "billing@analit.net";
			From = "billing@analit.net";
			Subject = "Не удалось отправить счет";
			PropertyBag["invoice"] = invoice;
		}

		public MonorailMailer RevisionAct(RevisionAct act, string emails, string comment)
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
			PropertyBag["comment"] = comment;
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
			PropertyBag["admin"] = SecurityContext.Administrator;

			return this;
		}

		public void NotifyAboutChanges(AuditableProperty property, object entity, string to)
		{
			var notificationAware = entity as IChangesNotificationAware;
			if (notificationAware != null && !notificationAware.ShouldNotify())
				return;

			var mailer = PropertyChanged(property, entity);
			mailer.To = to;
			mailer.Send();
		}

		private MonorailMailer PropertyChanged(AuditableProperty property, object entity)
		{
			var propertyMessage = property.Message;
			var notifyAware = property as INotificationAware;
			if (notifyAware != null)
				propertyMessage = notifyAware.NotifyMessage;

			//может быть null если property решило что не нужно отправлять
			//уведомления для этого изменения
			//например: регин который был добавлен помечен как не уведомляемый
			if (String.IsNullOrEmpty(propertyMessage))
				return this;

			Subject = String.Format("Изменено поле '{0}'", property.Name);
			if (property.IsHtml) {
				Template = "PropertyChanged_html";
				IsBodyHtml = true;
			}
			else {
				Template = "PropertyChanged_txt";
				IsBodyHtml = false;
			}
			var message = new StringBuilder();
			message.Append(GetAdditionalMessages(entity).ToString());
			if (propertyMessage.StartsWith("$$$"))
				propertyMessage = propertyMessage.Remove(0, 3);
			message.AppendLine(propertyMessage);

			GeneralizationPropertyChanged(entity, message.ToString());

			return this;
		}

		private void GeneralizationPropertyChanged(object entity, string message = null)
		{
			var idLabel = BindingHelper.TryGetDescription(NHibernateUtil.GetClass(entity), "Id");
			if (idLabel == null)
				idLabel = "Код " + GetEntityName(entity);
			From = "register@analit.net";
			PropertyBag["message"] = message;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["entity"] = entity;
			PropertyBag["type"] = Inflector.Pluralize(NHibernateUtil.GetClass(entity).Name);
			PropertyBag["idLabel"] = idLabel;
		}


		private string GetEntityName(object entity)
		{
			var type = NHibernateUtil.GetClass(entity);
			if (type == typeof(Client))
				return "клиента";
			if (type == typeof(Supplier))
				return "поставщика";
			if (type == typeof(LegalEntity))
				return "юр. Лица";
			if (type == typeof(User))
				return "пользователя";
			if (type == typeof(Payer))
				return "плательщика";
			if (type == typeof(News))
				return "новости";
			return "не оптеделено";
		}

		public MonorailMailer DeleteOrEditAct(Act act, string to, string messageSubject, bool isDelete)
		{
			From = "billing@analit.net";
			To = to;
			IsBodyHtml = true;
			Template = "ModifyAct";
			Subject = messageSubject;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["act"] = act;
			if(isDelete)
				PropertyBag["dateTime"] = String.Format("Дата и время удаления: {0}", DateTime.Now);
			else
				PropertyBag["dateTime"] = String.Format("Дата и время изменения: {0}", DateTime.Now);
			PropertyBag["messageSubject"] = messageSubject;
			return this;
		}

		public MonorailMailer RegisterOrDeleteNews(News news, string to, string messageSubject)
		{
			To = to;
			IsBodyHtml = true;
			Template = "RegisterNews";
			Subject = messageSubject;
			PropertyBag["messageSubject"] = messageSubject;
			PropertyBag["header"] = news.Header;
			PropertyBag["body"] = news.Body;
			PropertyBag["destination"] = news.DestinationType.GetDescription();
			GeneralizationPropertyChanged(news);
			return this;
		}

		private static StringBuilder GetAdditionalMessages(object entity)
		{
			var message = new StringBuilder();

			var user = entity as User;
			if (user != null) {
				message.AppendLine("Клиент " + user.Client.Name);
			}

			var news = entity as News;
			if(news != null) {
				message.AppendLine("Адресат: " + news.DestinationType.GetDescription() + "<br>");
				message.AppendLine("Дата и время публикации новости: " + news.PublicationDate + "<br>");
			}

			if (!(entity is Service))
				return message;

			var client = entity as Client;
			if (client != null) {
				message.AppendLine("Плательщики " + client.Payers.Implode(p => p.Name));
			}
			var supplier = entity as Supplier;
			if (supplier != null) {
				message.AppendLine("Плательщик " + supplier.Payer.Name);
			}
			return message;
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
			PropertyBag["admin"] = SecurityContext.Administrator;

			return this;
		}

		public MonorailMailer DeleteMiniMailToSupplier(string email, string body)
		{
			From = "office@analit.net";
			To = email;
			Subject = "Удаление письма минипочты";
			PropertyBag["body"] = body;
			Template = "DeleteMiniMail";

			return this;
		}

		public MonorailMailer DeleteMiniMailToOffice(Mail mail)
		{
			From = "office@analit.net";
			To = "a.zolotarev@analit.net";
			Subject = "Удаление письма минипочты";
			IsBodyHtml = true;
			var mailItem = new MailItem(mail);
			PropertyBag["body"] = string.Format("Удалено письмо минипочты поставщика '{0}' от {1}. </br> {2}", mail.Supplier.Name, mail.LogTime, mailItem.To);
			Template = "DeleteMiniMail";

			return this;
		}

		public MonorailMailer AccountChanged(Account account)
		{
			Template = "AccountChanged";

			var payer = account.Payer;
			var service = account.Service;

			From = "billing@analit.net";
			To = "billing@analit.net";
			Subject = String.Format("Изменение стоимости {0} - {1}", payer.Name, payer.Id);

			if (service != null) {
				Subject += String.Format(", {0} - {1}, {2}",
					service.Name,
					service.Id,
					BindingHelper.GetDescription(service.Type));
			}

			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["payer"] = payer;
			PropertyBag["service"] = service;
			PropertyBag["account"] = account;
			PropertyBag["newPayment"] = account.Payment;
			PropertyBag["oldPayment"] = account.OldValue(a => a.Payment);

			return this;
		}

		public MonorailMailer InvoiceToEmail(Invoice invoice, bool interactive)
		{
			try {
				SendInvoiceToEmail(invoice);

				if (!interactive)
					invoice.SendToEmail = false;

				if (_log.IsDebugEnabled)
					_log.DebugFormat("Счет {3} для плательщика {2} за {0} отправлен на адреса {1}",
						invoice.Period,
						invoice.Payer.GetInvocesAddress(),
						invoice.Payer.Name,
						invoice.Id);
			}
			catch (DoNotHaveContacts) {
				if (_log.IsDebugEnabled)
					_log.DebugFormat("Счет {0} не отправлен тк не задана контактная информация для плательщика {1} - {2}",
						invoice.Id,
						invoice.Payer.Id,
						invoice.Payer.Name);

				if (interactive) {
					DoNotHaveInvoiceContactGroup(invoice);
				}
				else if (invoice.ShouldNotify()) {
					invoice.LastErrorNotification = DateTime.Now;
					DoNotHaveInvoiceContactGroup(invoice);
				}
			}
			return this;
		}

		private void SendInvoiceToEmail(Invoice invoice)
		{
			var to = invoice.Payer.GetInvocesAddress();

			Template = "Invoice";
			Layout = "Print";
			IsBodyHtml = true;

			From = "billing@analit.net";
			To = to;
			Subject = String.Format("Счет за {0}", invoice.Period);

			PropertyBag["invoice"] = invoice;
			PropertyBag["inlineInvoice"] = true;
		}

		public void SendInvoiceToMinimail(Invoice invoice)
		{
			var to = invoice.Payer.ClientsMinimailAddressesAsString;

			Template = "Invoice";
			Layout = "Print";
			IsBodyHtml = true;

			From = "billing@analit.net";
			To = to;
			Subject = String.Format("Счет за {0}", invoice.Period);

			PropertyBag["invoice"] = invoice;
			PropertyBag["inlineInvoice"] = false;

			var memory = new MemoryStream();
			var writer = new StreamWriter(memory);
			RenderTemplate("/Invoices/InvoiceBody", writer);
			writer.Flush();
			memory.Position = 0;
			Attachments.Add(new Attachment(memory, "Счет.html"));
		}

		public MonorailMailer PayerDelete(Payer payer, string deleteComment)
		{
			GeneralizationPropertyChanged(payer, deleteComment.Replace("\r\n", "<br/>"));
			To = "BillingList@analit.net";
			IsBodyHtml = true;
			Template = "DeletePayer";
			Subject = string.Format("Удален плательщик {0}", payer.Name);
			PropertyBag["payer"] = payer;
			return this;
		}

		public MonorailMailer PayerRegistred(Payer payer)
		{
			GeneralizationPropertyChanged(payer, payer.Comment);
			To = "billing@analit.net, office@analit.net";
			IsBodyHtml = true;
			Template = "PropertyChanged_html";
			Subject = String.Format("Зарегистрирован {0}", BindingHelper.GetDescription(payer).ToLower());
			return this;
		}
	}
}