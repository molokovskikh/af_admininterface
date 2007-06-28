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
	[Layout("default")]
	public class BillingController : SmartDispatcherController
	{
		public void Edit(uint clientCode)
		{
			BillingInstance billingInstance = BillingInstance.GetByClientCode(clientCode);
			ClientMessage clientMessage = ClientMessage.TryFind(clientCode);
			if (clientMessage != null)
				PropertyBag.Add("ClientMessage", clientMessage);

			PropertyBag.Add("ClientCode", clientCode);
			PropertyBag.Add("Instance", billingInstance);
			PropertyBag["ContactGroups"] = billingInstance.ContactGroupOwner.ContactGroups;
			SetTitle(billingInstance);
		}

		public void Update([DataBind("Instance")] BillingInstance billingInstance, uint clientCode)
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
