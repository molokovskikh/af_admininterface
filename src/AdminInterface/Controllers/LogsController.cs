using System;
using AdminInterface.Filters;
using AdminInterface.Helpers;
using AdminInterface.Model;
using AdminInterface.Models.Logs;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("logs"), Helper(typeof(BindingHelper)), Helper(typeof(ViewHelper))]
	[Filter(ExecuteEnum.BeforeAction, typeof(AuthorizeFilter))]
	public class LogsController : SmartDispatcherController
	{
		public void DocumentLog(uint clientCode)
		{
			DocumentLog(clientCode, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void DocumentLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			Client client = Client.Find(clientCode);

			PropertyBag["logEntities"] = DocumentLogEntity.GetEnitiesForClient(client,
			                                                                   beginDate,
			                                                                   endDate.AddDays(1));
			PropertyBag["client"] = client;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void ShowUpdateDetails(uint updateLogEntityId)
		{
			DateTime begin = DateTime.Now;
			var logEntity = UpdateLogEntity.Find(updateLogEntityId);
			PropertyBag["updateLogEntityId"] = logEntity.Id;
			var detailsLogEntities = logEntity.UpdateDownload;
			PropertyBag["detailLogEntities"] = detailsLogEntities;

			ulong totalByteDownloaded = 0;
			ulong totalBytes = 1;
			foreach (var entity in detailsLogEntities)
			{
				totalByteDownloaded += entity.SendBytes;
				totalBytes = entity.TotalBytes;
			}

			System.Diagnostics.Trace.WriteLine(begin - DateTime.Now);
			PropertyBag["allDownloaded"] = totalByteDownloaded >= totalBytes;
		}

		public void ShowDownloadLog(uint updateLogEntityId)
		{
			PropertyBag["updateLogEnriryId"] = updateLogEntityId;
			PropertyBag["log"] = AnalitFDownloadLogEntity.Find(updateLogEntityId).Log;
		}

		public void UpdateLog(uint clientCode)
		{
			UpdateLog(clientCode, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void UpdateLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			PropertyBag["logEntities"] = UpdateLogEntity.GetEntitiesFormClient(clientCode, 
																			   beginDate, 
																			   endDate.AddDays(1));

			PropertyBag["client"] = Client.Find(clientCode);
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void PasswordChangeLog(string login)
		{
			PasswordChangeLog(login, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void PasswordChangeLog(string login, DateTime beginDate, DateTime endDate)
		{
			PropertyBag["logEntities"] = PasswordChangeLogEntity.GetByLogin(login, beginDate, endDate);
			PropertyBag["login"] = login;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}
	}
}
