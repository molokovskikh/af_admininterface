using System;
using AdminInterface.Helpers;
using AdminInterface.Model;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("logs"), Helper(typeof(BindingHelper)), Helper(typeof(ViewHelper))]
	[Filter(ExecuteWhen.BeforeAction, typeof(AuthorizeFilter))]
	public class LogsController : SmartDispatcherController
	{
		public void DocumentLog(uint clientCode)
		{
		    DocumentLog(clientCode, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void DocumentLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			var client = Client.Find(clientCode);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			PropertyBag["logEntities"] = DocumentLogEntity.GetEnitiesForClient(client,
			                                                                   beginDate,
			                                                                   endDate.AddDays(1));
			PropertyBag["client"] = client;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void ShowUpdateDetails(uint updateLogEntityId)
		{
			var logEntity = UpdateLogEntity.Find(updateLogEntityId);
			var detailsLogEntities = logEntity.UpdateDownload;

			ulong totalByteDownloaded = 0;
			ulong totalBytes = 1;
			foreach (var entity in detailsLogEntities)
			{
				totalByteDownloaded += entity.SendBytes;
				totalBytes = entity.TotalBytes;
			}

			PropertyBag["updateLogEntityId"] = logEntity.Id;
			PropertyBag["detailLogEntities"] = detailsLogEntities;
			PropertyBag["allDownloaded"] = totalByteDownloaded >= totalBytes;
		}

		public void ShowDownloadLog(uint updateLogEntityId)
		{
			PropertyBag["updateLogEnriryId"] = updateLogEntityId;
			PropertyBag["log"] = UpdateLogEntity.Find(updateLogEntityId).Log;
		}

		public void UpdateLog(uint clientCode)
		{
			UpdateLog(clientCode, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void UpdateLog(uint clientCode, DateTime beginDate, DateTime endDate)
		{
			var client = Client.Find(clientCode);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			PropertyBag["logEntities"] = UpdateLogEntity.GetEntitiesFormClient(client.Id, 
																			   beginDate, 
																			   endDate.AddDays(1));

			PropertyBag["client"] = client;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void PasswordChangeLog(string login)
		{
			PasswordChangeLog(login, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void PasswordChangeLog(string login, DateTime beginDate, DateTime endDate)
		{
			var user = User.GetByLogin(login);
			SecurityContext.Administrator.CheckClientType(user.Client.Type);
			SecurityContext.Administrator.CheckClientHomeRegion(user.Client.HomeRegion.Id);

			PropertyBag["logEntities"] = PasswordChangeLogEntity.GetByLogin(user.Login,
			                                                                beginDate,
			                                                                endDate.AddDays(1));
			PropertyBag["login"] = login;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		[RequiredPermission(PermissionType.MonitorUpdates)]
		public void ClientRegistrationLog()
		{
			ClientRegistrationLog(DateTime.Today.AddDays(-1), DateTime.Today, 0);
		}

		[RequiredPermission(PermissionType.MonitorUpdates)]
		public void ClientRegistrationLog(DateTime beginDate, DateTime endDate, int dayWithoutUpdate)
		{
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
			PropertyBag["dayWithoutUpdate"] = dayWithoutUpdate;
			PropertyBag["logEntities"] = ClientRegistrationLogEntity.GetEntitiesForPeriond(beginDate,
			                                                                               endDate.AddDays(1),
			                                                                               dayWithoutUpdate);
		}
	}
}
