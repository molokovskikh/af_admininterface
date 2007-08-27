using System;
using AdminInterface.Helpers;
using AdminInterface.Model;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("logs"), Helper(typeof(BindingHelper)), Helper(typeof(ViewHelper))]
	public class LogsController : SmartDispatcherController
	{
		public void DocumentLog(uint clientCode)
		{
			DocumentLog(clientCode, DateTime.Now.AddDays(-1), DateTime.Now);
		}

		public void DocumentLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			Client client = Client.Find(clientCode);

			PropertyBag["logEntities"] = DocumentLogEntity.GetEnitiesForClient(client,
			                                                                          beginDate,
			                                                                          endDate);
			PropertyBag["client"] = client;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void ShowUpdateDetails(uint updateLogEntityId)
		{
			UpdateLogEntity logEntity = UpdateLogEntity.Find(updateLogEntityId);
			PropertyBag["updateLogEntityId"] = logEntity.Id;
			PropertyBag["detailLogEntities"] = InternetLogEntity.GetUpdateSession(logEntity.UserName, logEntity.RequestTime, logEntity.Id);
		}

		public void UpdateLog(uint clientCode)
		{
			UpdateLog(clientCode, DateTime.Now.AddDays(-1), DateTime.Now);
		}

		public void UpdateLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			PropertyBag["logEntities"] = UpdateLogEntity.GetEntitiesFormClient(clientCode, 
																			   beginDate, 
																			   endDate);

			PropertyBag["client"] = Client.Find(clientCode);
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}
	}
}
