using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.Core.Smtp;
using Common.Tools;
using Common.Web.Ui.Helpers;
using DiffMatchPatch;
using ExcelLibrary.SpreadSheet;
using NHibernate;
using log4net;

namespace AdminInterface.Models
{
	public class MonorailMailer : BaseMailer
	{
		private static ILog _log = LogManager.GetLogger(typeof (MonorailMailer));

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

		public MonorailMailer EnableChanged(IEnablable item)
		{
			Template = "EnableChanged";
			To = "RegisterList@subscribe.analit.net";
			From = "register@analit.net";
			var lastDisable = "неизвестно";

			var type = "";
			var clazz = NHibernateUtil.GetClass(item);
			if (clazz == typeof(User))
			{
				type = "пользователя";
				var user = (User) item;
				PropertyBag["service"] = user.RootService;
				var disable = UserLogRecord.LastOff(user.Id);
				if (disable != null)
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
			}
			if (clazz == typeof(Address))
			{
				type = "адреса";
				var address = (Address) item;
				PropertyBag["service"] = address.Client;
				var disable = AddressLogRecord.LastOff(address.Id);
				if (disable != null)
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
			}
			if (clazz == typeof(Client))
			{
				type = "клиента";
				var client = Client.Find(((Service)item).Id); //(Client) item;
				PropertyBag["service"] = client;
				var disable = ClientLogRecord.LastOff(client);
				if (disable != null)
					lastDisable = String.Format("{0} пользователем {1}", disable.LogTime, disable.OperatorName);
			}
			if (clazz == typeof(Supplier))
			{
				type = "поставщика";
				PropertyBag["service"] = item;
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
			PropertyBag["user"] = client.Users.First();
			PropertyBag["admin"] = SecurityContext.Administrator;
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

			return this;
		}

		public void NotifyAboutChanges(AuditableProperty property, object entity)
		{
			MonorailMailer mailer = null;
			if (entity is Service) {
				mailer = NotifyAboutServiceChanges(property, entity);
			}

			if (mailer != null)
				mailer.Send();
		}

		public MonorailMailer NotifyPropertyDiff(AuditableProperty property, object entity)
		{
			To = "RegisterList@subscribe.analit.net";
			From = "register@analit.net";
			Subject = String.Format("Изменено поле '{0}'", property.Name);
			IsBodyHtml = true;
			Template = "PropertyChanges";
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["message"] = property.Message.Remove(0, 3);
			PropertyBag["name"] = property.Name;
			PropertyBag["payer"] = entity;

			return this;
		}

		public MonorailMailer NotifyAboutServiceChanges(AuditableProperty property, object entity)
		{
			var message = new StringBuilder();
			var id = ((Service)entity).Id;
			message.AppendLine("Клиент " + id);
			var client = entity as Client;
			if (client != null) {
				message.AppendLine("Плательщики: " + client.Payers.Implode(p => p.Name));
				if (client.Payers.Any( p => p.PayerID == 921))
					return null;
			}
			var supplier = entity as Supplier;
			if (supplier != null) {
				message.AppendLine("Плательщик: " + supplier.Payer.Name);
				if (supplier.Payer.PayerID == 921)
					return null;
			}
			message.AppendLine(property.Message.Remove(0, 3));

			Template = "ChangeNameFullName";
			From = "register@analit.net";
			Subject = String.Format("Изменено поле '{0}'", property.Name);
			To = "RegisterList@subscribe.analit.net";
			PropertyBag["message"] = message;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["name"] = property.Name;

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

		public MonorailMailer AccountingChanged(Account account)
		{
			Template = "AccountingChanged";

			var payer = account.Payer;
			var service = account.Service;

			From = "billing@analit.net";
			To = "billing@analit.net";
			Subject = String.Format("Изменение стоимости {0} - {1}", payer.Name, payer.Id);

			if (service != null)
			{
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
			try
			{
				SendInvoiceToEmail(invoice);

				if (interactive)
					invoice.SendToEmail = false;

				if (_log.IsDebugEnabled)
					_log.DebugFormat("Счет {3} для плательщика {2} за {0} отправлен на адреса {1}",
						invoice.Period,
						invoice.Payer.GetInvocesAddress(),
						invoice.Payer.Name,
						invoice.Id);
			}
			catch (DoNotHaveContacts)
			{
				if (_log.IsDebugEnabled)
					_log.DebugFormat("Счет {0} не отправлен тк не задана контактная информация для плательщика {1} - {2}",
						invoice.Id,
						invoice.Payer.Id,
						invoice.Payer.Name);

				if (interactive)
				{
					DoNotHaveInvoiceContactGroup(invoice);
				}
				else if (invoice.ShouldNotify())
				{
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
	}
}