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
using Billing.Model;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using NHibernate;

namespace Billing.Controllers
{
	[Layout("default")]
	public class BillingController : SmartDispatcherController
	{
		public void Edit(uint clientCode, uint billingCode)
		{
			BillingInstance billingInstance = BillingInstance.GetById(billingCode);
			PropertyBag.Add("Instance", billingInstance);
			SetTitle(billingInstance);
		}

		public void Update([DataBind("Instance")] BillingInstance billingInstance)
		{
			billingInstance.Update();
			PropertyBag.Add("UpdateMessage", "Изменения сохранены");
			PropertyBag.Add("Instance", billingInstance);
			SetTitle(billingInstance);
			RenderView("Edit");
		}

		public void SendMessage([DataBind("Instance")] BillingInstance instance, string message, uint showCount)
		{
			ISessionFactoryHolder holder = ActiveRecordMediator.GetSessionFactoryHolder();
			ISession session = holder.CreateSession(typeof (void));
				try
				{
					session.BeginTransaction(IsolationLevel.ReadCommitted);
					IDbCommand command = session.Connection.CreateCommand();
					command.CommandText = @"
UPDATE usersettings.retclientsset 
        SET ShowMessageCount=?ShowCount, 
        Message             =?Message 
WHERE   clientcode          =?ClientCode;";
					IDbDataParameter parameter = command.CreateParameter();
					parameter.ParameterName = "?ShowCount";
					parameter.Value = showCount;
					command.Parameters.Add(parameter);

					parameter = command.CreateParameter();
					parameter.ParameterName = "?Message ";
					parameter.Value = message;
					command.Parameters.Add(parameter);

					parameter = command.CreateParameter();
					parameter.ParameterName = "?ClientCode";
					parameter.Value = instance.PayerID;
					command.Parameters.Add(parameter);

					command.ExecuteNonQuery();
					session.Flush();
					session.Transaction.Commit();
				}
				catch(Exception)
				{
					session.Transaction.Rollback();
					throw;
				}
				finally
				{
					holder.ReleaseSession(session);
				}

			PropertyBag.Add("SendMessage", "Сообщение отправленно");
			RedirectToAction("Edit", "payerID=" + instance.PayerID);
		}

		private void SetTitle(BillingInstance billingInstance)
		{
			PropertyBag.Add("Title", String.Format("Детаьная информация о платильщике {0}", 
												   billingInstance.ShortName));
		}

		public bool IsContainsNotSendedMessage()
		{
			return true;
		}
	}
}
