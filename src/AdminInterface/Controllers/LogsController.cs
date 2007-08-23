using System;
using AdminInterface.Model;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[Layout("logs"), Helper(typeof(BindingHelper))]
	public class LogsController : SmartDispatcherController
	{
		public void DocumentLog(uint clientCode)
		{
			DocumentLog(clientCode, DateTime.Now.AddDays(-1), DateTime.Now);
		}

		public void DocumentLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			PropertyBag["logEntities"] = DocumentRecieveLogEntity.GetEnitiesForClient(clientCode, 
																					  beginDate, 
																					  endDate);
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
			PropertyBag["clientCode"] = clientCode;
		}

		public void ShowUpdateDetails(string userName, DateTime beginDate, DateTime endDate)
		{
			PropertyBag["logEntities"] = InternetLogEntity.GetUpdateSession(userName, beginDate, endDate);
		}
	}
}
