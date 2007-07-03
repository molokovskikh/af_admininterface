using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AdminInterface.Helpers;
using AdminInterface.Model;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using NHibernate;

namespace AdminInterface.Controllers
{
	[Layout("billing"), Helper(typeof(BindingHelper))]
	public class BillingController : ARSmartDispatcherController
	{
		public void Edit(uint clientCode)
		{
			Client client = Client.Find(clientCode);
			ClientMessage clientMessage = ClientMessage.TryFind(clientCode);

			if (clientMessage != null)
				PropertyBag.Add("ClientMessage", clientMessage);

			PropertyBag["LogRecords"] = ClientLogRecord.GetClientLogRecords(clientCode);
			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = client.BillingInstance;
			PropertyBag["ContactGroups"] = client.BillingInstance.ContactGroupOwner.ContactGroups;

			SetTitle(client.BillingInstance);
		}

		public void Update([ARDataBind("Instance", AutoLoadBehavior.Always)] BillingInstance billingInstance, uint clientCode)
		{
			billingInstance.UpdateAndFlush();
			Flash.Add("UpdateMessage", "Изменения сохранены");
			RedirectToAction("Edit", "clientCode=" + clientCode);
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage)
		{
			try
			{
				clientMessage.UpdateAndFlush();
				Flash.Add("SendMessage", "Сообщение отправленно");
			}
			catch(ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}
			RedirectToAction("Edit", "clientCode=" + clientMessage.ClientCode);
		}

		public void ChangeClientState(uint clientCode)
		{
			Client client = Client.Find(clientCode);
			if (client.Status == ClientStatus.On)
				client.Status = ClientStatus.Off;
			else
				client.Status = ClientStatus.On;
			DbLogHelper.SavePersistentWithLogParams(Session["UserName"].ToString(), HttpContext.Current.Request.UserHostAddress, client);
			RedirectToAction("Edit", "clientCode=" + clientCode);
		}

		public string GetChangeStatusButtonText(Client client)
		{
			if (client.Status == ClientStatus.On)
				return "Отключить клиента";
			else
				return "Включить клиента";
		}

		private void SetTitle(BillingInstance billingInstance)
		{
			PropertyBag.Add("Title", String.Format("Детальная информация о платильщике {0}",
												   billingInstance.ShortName));
		}

		public bool IsContainsNotShowedMessage()
		{
			return ((ClientMessage) PropertyBag["ClientMessage"]).ShowMessageCount > 0;
		}
	}
}
