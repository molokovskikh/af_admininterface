using System;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.Components.Common.EmailSender;
using Common.Web.Ui.Helpers;

namespace AdminInterface.MonoRailExtentions
{
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
	}
}