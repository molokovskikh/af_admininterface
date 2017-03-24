using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models;
using AdminInterface.Models.Certificates;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Dapper;
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
		public void Resend(uint[] ids, bool? statMode)
		{
			List<DocumentSendLog> logs;
			if(statMode.HasValue && statMode.Value)
				logs = DbSession.Query<DocumentReceiveLog>()
					.Where(l => ids.Contains(l.Id))
					.SelectMany(l => l.SendLogs).ToList();
			else
				logs = DbSession.Query<DocumentSendLog>().Where(d => ids.Contains(d.Id)).ToList();
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
			if (filter.Client != null) {
				var clientUsers = DbSession.Load<Client>(filter.Client.Id).Users;
				if (clientUsers != null) {
					filter.StatMode = clientUsers.Count > 1;
				}
			}
			var sqlFormat = @"
    SELECT
        document_logs.RowId as Id,
        document_logs.LogTime as LogTime,
        document_logs.DocumentType as DocumentType,
        document_logs.FileName as FileName,
        document_logs.Addition as Addition,
        document_logs.DocumentSize as DocumentSize,
        analitFUpdates.UpdateId as SendUpdateId,          
        documentHeaders.ProviderDocumentId as ProviderDocumentId,
        documentHeaders.DocumentDate as DocumentDate, 
        
       IFNULL(documentHeaders.Id,IFNULL(reject.Id,NULL)) as DocumentId,
       IFNULL(documentHeaders.WriteTime,IFNULL(reject.WriteTime,NULL))   as DocumentWriteTime,
       IFNULL(documentHeaders.Parser,IFNULL(reject.Parser,NULL))   as Parser,
        
        services.Name as Supplier,
        suppliers.Id as SupplierId,
        servClients.Name as Client,
        clients.Id as ClientId,
        addresses.Address as Address,
        addresses.Enabled as AddressEnabled,
        services.HomeRegion as RegionName
{0}

    FROM
        Logs.Document_logs document_logs 
    inner join
        Customers.Suppliers suppliers 
            on document_logs.FirmCode=suppliers.Id 
    left outer join
        Customers.Services services 
            on suppliers.Id=services.Id 
    left outer join
        Customers.Clients clients 
            on document_logs.ClientCode=clients.Id 
    left outer join
        Customers.Services servClients 
            on clients.Id=servClients.Id 
    left outer join
        Customers.Addresses addresses 
            on document_logs.AddressId=addresses.Id 
    left outer join
        logs.AnalitFUpdates analitFUpdates 
            on document_logs.SendUpdateId=analitFUpdates.UpdateId 
{1}
    left outer join
        documents.DocumentHeaders documentHeaders 
            on document_logs.RowId=documentHeaders.DownloadId 
    left outer join
        documents.rejectheaders reject 
            on document_logs.RowId=reject.DownloadId  
    WHERE
        document_logs.LogTime >= @LogTimeBegin 
        and document_logs.LogTime <= @LogTimeEnd 
{2}
    ORDER BY
        LogTime desc; 
";


			PropertyBag["filter"] = filter;

			var statModeFrom = "";
			var statModeSelect = "";

			if (!filter.StatMode) {
				statModeSelect = @",
        users.Login as Login,
        users.Id as LoginId,
        logsAnalitFUpdates.RequestTime as RequestTime,
        documentSendLogs.Id DeliveredId,
        documentSendLogs.FileDelivered as FileDelivered,
        documentSendLogs.DocumentDelivered as DocumentDelivered,
        documentSendLogs.SendDate as SendDate 
";

				statModeFrom = @"
    left outer join
        Logs.DocumentSendLogs documentSendLogs 
            on document_logs.RowId=documentSendLogs.DocumentId 
    left outer join
        Customers.Users users 
            on documentSendLogs.UserId=users.Id 
    left outer join
        logs.AnalitFUpdates logsAnalitFUpdates 
            on documentSendLogs.UpdateId=logsAnalitFUpdates.UpdateId 
";
			}

			// если выставлен флаг "только не разобранные накладные" (OnlyNoParsed, для соответствующей страницы), используем фильтр, иначе sql прямой запрос
			if (filter.OnlyNoParsed) {
				PropertyBag["logEntities"] = filter.Find(DbSession);
			} else {
				var documentLogList = new List<DocumentLog>();
				if (filter.Supplier != null) {
					documentLogList =
						DbSession.Connection.Query<DocumentLog>(
							string.Format(sqlFormat, statModeSelect, statModeFrom, "and document_logs.FirmCode = @SupplierId "),
							new {
								@LogTimeBegin = filter.Period.Begin,
								@LogTimeEnd = filter.Period.End.AddDays(1).AddSeconds(-1),
								@SupplierId = filter.Supplier.Id
							}).ToList();
				}
				if (filter.Client != null) {
					documentLogList =
						DbSession.Connection.Query<DocumentLog>(
							string.Format(sqlFormat, statModeSelect, statModeFrom, "and document_logs.ClientCode = @ClientId "),
							new {
								@LogTimeBegin = filter.Period.Begin,
								@LogTimeEnd = filter.Period.End.AddDays(1).AddSeconds(-1),
								@ClientId = filter.Client.Id
							}).ToList();
				}
				if (filter.User != null) {
					documentLogList =
						DbSession.Connection.Query<DocumentLog>(
							string.Format(sqlFormat, statModeSelect, statModeFrom, "and documentSendLogs.UserId = @UserId "),
							new {
								@LogTimeBegin = filter.Period.Begin,
								@LogTimeEnd = filter.Period.End.AddDays(1).AddSeconds(-1),
								@UserId = filter.User.Id
							}).ToList();
				}
				PropertyBag["logEntities"] = documentLogList;
			}
		}

		public void DocumentsToExcel([DataBind("filter")] DocumentFilter filter)
		{
			var result = ExportModel.DocumentsLog(DbSession, filter);
			this.RenderFile("Неразобранные накладные.xls", result);
		}

		public void Download(uint id)
		{
			var log = DbSession.Load<DocumentReceiveLog>(id);

			var file = log.GetRemoteFileName(Config);
			this.RenderFile(file, log.FileName);
		}

		public void ShowStatDetails(uint documentLogId)
		{
			CancelLayout();
			PropertyBag["documentLogId"] = documentLogId;
			PropertyBag["items"] = DbSession.Load<DocumentReceiveLog>(documentLogId).SendLogs;
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

		public void ShowRejectDetails(uint documentLogId, uint? supplierId)
		{
			ShowDocumentDetails(documentLogId, supplierId);
		}

		public void ShowDocumentDetails(uint documentLogId, uint? supplierId)
		{
			CancelLayout();

			var documentLog = DbSession.Load<DocumentReceiveLog>(documentLogId);
			PropertyBag["documentLogId"] = documentLogId;
			PropertyBag["documentLog"] = documentLog;
			PropertyBag["ShowDetails"] = documentLog.DocumentType == DocumentType.Waybill
				? "/Documents/ShowDocumentDetails"
				: "ShowRejectDetails";
			PropertyBag["filterSupplierId"] = supplierId;
		}

		public void CertificatesForReject(uint id, uint? clientId, uint? filterSupplierId)
		{
			CancelLayout();

			var line = DbSession.Load<RejectLine>(id);
			PropertyBag["certificates"] = null;
			var query = DbSession.CreateSQLQuery(String.Format(@"select value from catalogs.propertyvalues pv
join catalogs.productproperties p on p.PropertyValueId = pv.Id and p.ProductId = {0}", line.ProductEntity.Id));
			var properties = String.Join(", ", query.List<string>());
			if (!String.IsNullOrEmpty(properties)) {
				PropertyBag["productProperties"] = ", " + properties;
			}
			PropertyBag["line"] = line;
			PropertyBag["clientId"] = clientId;
			PropertyBag["filterSupplierId"] = filterSupplierId;
		}
		public void Certificates(uint id, uint? clientId, uint? filterSupplierId)
		{
			CancelLayout();

			var line = DbSession.Load<DocumentLine>(id);
			var certificatesSource = DbSession.Load<FullDocument>(line.Document.Id).Supplier.CertificateSource;
			PropertyBag["certificates"] = null;
			if (line.Certificate?.Files != null) {
				PropertyBag["certificates"] = line.Certificate.Files.Where(x => x.CertificateSourceId == certificatesSource.Id.ToString()).ToList();
			}

			var query = DbSession.CreateSQLQuery(String.Format(@"select value from catalogs.propertyvalues pv
join catalogs.productproperties p on p.PropertyValueId = pv.Id and p.ProductId = {0}", line.CatalogProduct.Id));
			var properties = String.Join(", ", query.List<string>());
			if(!String.IsNullOrEmpty(properties)) {
				PropertyBag["productProperties"] = ", " + properties;
			}
			PropertyBag["line"] = line;
			PropertyBag["clientId"] = clientId;
			PropertyBag["filterSupplierId"] = filterSupplierId;
		}

		public void Converted(uint id, uint? clientId)
		{
			CancelLayout();
			var convertedLine = "Позиция отсутствует в ассортиментном ПЛ";

			if (clientId != null) {
				var client = DbSession.Load<Client>(clientId);
				var line = DbSession.Load<DocumentLine>(id);
				if (client.Settings.AssortimentPrice != null && line.CatalogProduct != null) {
					uint? producerId = null;
					if(line.CatalogProducer != null)
						producerId = line.CatalogProducer.Id;
					var core = DbSession.Query<Core>()
						.Where(c => c.Price.Id == client.Settings.AssortimentPrice.Id
							&& c.ProductId == line.CatalogProduct.Id
							&& c.CodeFirmCr == producerId)
						.OrderBy(c => c.Code)
						.FirstOrDefault();
					if (core != null) {
						convertedLine = core.Code + " ";
						if (core.ProductSynonym != null)
							convertedLine += core.ProductSynonym.Synonym;
						convertedLine += "<br>" + core.CodeCr + " ";
						if (core.ProducerSynonym != null)
							convertedLine += core.ProducerSynonym.Synonym;
					}
				}
				else if(line.CatalogProduct != null) {
					Producer producer = null;
					if(line.CatalogProducer != null)
						producer = DbSession.Query<Producer>().FirstOrDefault(p => p.Id == line.CatalogProducer.Id);
					var product = DbSession.Query<Catalog>().FirstOrDefault(p => p.Id == line.CatalogProduct.Catalog.Id);
					if(product != null || producer != null) {
						convertedLine = (product == null ? "" : (product.Name + "<br>"))
							+ (producer == null ? "" : producer.Name);
					}
					PropertyBag["notFindAssortment"] = "* не указан ассортиментный прайс-лист для конвертации";
				}
			}
			PropertyBag["convertedLine"] = convertedLine;
			PropertyBag["lineId"] = id;
		}

		public void Certificate(uint id)
		{
			var file = DbSession.Load<CertificateFile>(id);
			this.RenderFile(file.GetStorageFileName(Config), file.Filename);
		}

		/// <summary>
		/// Получение детальной информации о логе в html виде, для таблицы логов
		/// </summary>
		/// <param name="updateLogEntityId">Идентификатор лога</param>
		public void ShowDownloadLog(uint updateLogEntityId)
		{
			CancelLayout();

			PropertyBag["id"] = updateLogEntityId;
			PropertyBag["log"] = DbSession.Load<UpdateLogEntity>(updateLogEntityId).Log;
		}

		public void UpdateLog(UpdateType? updateType, ulong regionMask, uint? clientCode, uint? userId)
		{
			//Эти данные вообще нафиг не нужны - все итак возьмется из фильтра
			UpdateLog(updateType, regionMask, clientCode, userId, DateTime.Today, DateTime.Today.AddDays(1));
		}

		/// <summary>
		/// История обновлений для пользователей новой версии analit-f
		/// </summary>
		public void NewUpdateLog()
		{
			SetSmartBinder(AutoLoadBehavior.NullIfInvalidKey);
			//по умолчанию биндер будет пытаться проверить наши обекты, в данном контексте делать этого не следует
			Binder.Validator = null;

			var filter = new NewUpdateFilter();
			BindObjectInstance(filter, "filter");
			PropertyBag["filter"] = filter;

			PropertyBag["logEntities"] = filter.Find(DbSession);
		}

		public void ShowClientLog(uint id)
		{
			CancelLayout();

			var log = DbSession.Get<RequestLog>(id);
			var text = "";
			if (log != null && log.RequestToken != null) {
				text = DbSession.Query<ClientAppLog>()
					.Where(x => x.RequestToken == log.RequestToken)
					.Select(x => x.Text)
					.FirstOrDefault();
			}
			else {
				text = DbSession.Get<ClientAppLog>(id).Text;
			}
			PropertyBag["id"] = id;
			PropertyBag["log"] = text;
			RenderView("ShowDownloadLog");
		}

		public void UpdateLog(UpdateType? updateType, ulong? regionMask, uint? clientCode, uint? userId,
			DateTime beginDate, DateTime endDate)
		{
			SetSmartBinder(AutoLoadBehavior.NullIfInvalidKey);
			//по умолчанию биндер будет пытаться проверить наши обекты, в данном контексте делать этого не следует
			Binder.Validator = null;

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
			PropertyBag["logEntities"] = filter.Find(DbSession);
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