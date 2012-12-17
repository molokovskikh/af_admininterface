﻿using System;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Helper(typeof(PaginatorHelper), "paginator"),
		Secure
	]
	public class LogsController : AdminInterfaceController
	{
		public LogsController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Resend(uint[] ids)
		{
			var logs = DbSession.Query<DocumentSendLog>().Where(d => ids.Contains(d.Id));
			foreach (var log in logs) {
				log.SendedInUpdate = null;
				log.Committed = false;
				DbSession.Save(log);
			}
			Notify("Документы будут отправлены повторно");
			RedirectToReferrer();
		}

		public void Documents([ARDataBind("filter", AutoLoadBehavior.NullIfInvalidKey)] DocumentFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["logEntities"] = filter.Find();
		}

		public void Download(uint id)
		{
			var log = DbSession.Load<DocumentReceiveLog>(id);

			var file = log.GetRemoteFileName(Config);
			this.RenderFile(file, log.FileName);
		}

		public void ShowUpdateDetails(uint updateLogEntityId)
		{
			CancelLayout();

			var logEntity = DbSession.Load<UpdateLogEntity>(updateLogEntityId);
			var detailsLogEntities = logEntity.UpdateDownload;
			var detailDocumentLogs = logEntity.GetLoadedDocumentLogs();

			ulong totalByteDownloaded = 0;
			ulong totalBytes = 1;
			foreach (var entity in detailsLogEntities) {
				totalByteDownloaded += entity.SendBytes;
				totalBytes = entity.TotalBytes;
			}

			PropertyBag["updateLogEntityId"] = logEntity.Id;
			PropertyBag["logEntity"] = logEntity;
			PropertyBag["detailLogEntities"] = detailsLogEntities;
			PropertyBag["allDownloaded"] = totalByteDownloaded >= totalBytes;
			PropertyBag["detailDocumentLogs"] = detailDocumentLogs;
		}

		public void ShowDocumentDetails(uint documentLogId)
		{
			CancelLayout();

			var documentLog = DbSession.Load<DocumentReceiveLog>(documentLogId);
			PropertyBag["documentLogId"] = documentLogId;
			PropertyBag["documentLog"] = documentLog;
		}

		public void Certificates(uint id)
		{
			CancelLayout();

			var line = DbSession.Load<DocumentLine>(id);
			PropertyBag["line"] = line;
		}

		public void Converted(uint id, uint? userId, uint? clientId)
		{
			if(userId != null) {
				var user = DbSession.Load<User>(userId);
				if(user.Client != null)
					clientId = user.Client.Id;
			}
			if(clientId != null) {
				var client = DbSession.Load<Client>(clientId);
				var line = DbSession.Load<DocumentLine>(id);
				if(client.Settings.AssortimentPrice != null && line.CatalogProduct != null) {
					var core = DbSession.Query<Core>()
						.Where(c => c.Price.Id == client.Settings.AssortimentPrice.Id
							&& c.ProductId == line.CatalogProduct.Id
							&& c.CodeFirmCr == line.CatalogProducer.Id)
						.OrderBy(c => c.Code)
						.FirstOrDefault();
					if(core != null)
						PropertyBag["convertedLine"] = core.Code + " "
							+ core.ProductSynonym.Synonym + " " + core.CodeCr
							+ " " + core.ProducerSynonym.Synonym;
				}
			}
		}

		public void Certificate(uint id)
		{
			var file = DbSession.Load<CertificateFile>(id);
			this.RenderFile(file.GetStorageFileName(Config), file.Filename);
		}

		public void ShowDownloadLog(uint updateLogEntityId)
		{
			CancelLayout();

			PropertyBag["updateLogEnriryId"] = updateLogEntityId;
			PropertyBag["log"] = DbSession.Load<UpdateLogEntity>(updateLogEntityId).Log;
		}

		public void UpdateLog(UpdateType? updateType, ulong regionMask, uint? clientCode, uint? userId)
		{
			UpdateLog(updateType, regionMask, clientCode, userId, DateTime.Today, DateTime.Today.AddDays(1));
		}

		public void UpdateLog(UpdateType? updateType, ulong? regionMask, uint? clientCode, uint? userId,
			DateTime beginDate, DateTime endDate)
		{
			var filter = new UpdateFilter();
			filter.BeginDate = beginDate;
			filter.EndDate = endDate;

			if (updateType.HasValue) {
				filter.UpdateType = updateType;
				filter.RegionMask = Admin.RegionMask;

				if (regionMask.HasValue)
					filter.RegionMask &= regionMask.Value;
			}
			if (clientCode.HasValue)
				filter.Client = DbSession.Load<Client>(clientCode.Value);
			else if (userId.HasValue)
				filter.User = DbSession.Load<User>(userId.Value);

			BindObjectInstance(filter, "filter");

			if (filter.Client != null)
				filter.Client = DbSession.Load<Client>(filter.Client.Id);

			if (filter.User != null)
				filter.User = DbSession.Load<User>(filter.User.Id);

			PropertyBag["beginDate"] = filter.BeginDate;
			PropertyBag["endDate"] = filter.EndDate;
			PropertyBag["filter"] = filter;
			PropertyBag["logEntities"] = filter.Find();
		}

		public void Statistics([SmartBinder] StatisticsFilter filter)
		{
			PropertyBag["Statistics"] = filter.Find();
			PropertyBag["filter"] = filter;
		}

		public void PasswordChangeLog(uint id)
		{
			PasswordChangeLog(id, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void PasswordChangeLog(uint id, DateTime beginDate, DateTime endDate)
		{
			var user = DbSession.Load<User>(id);

			PropertyBag["logEntities"] = PasswordChangeLogEntity.GetByLogin(user.Login,
				beginDate,
				endDate.AddDays(1));
			PropertyBag["login"] = user.Login;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void Orders([SmartBinder] OrderFilter filter)
		{
			if (filter.Client == null && filter.User != null)
				filter.Client = filter.User.Client;

			PropertyBag["orders"] = filter.Find();
			PropertyBag["filter"] = filter;
		}
	}
}