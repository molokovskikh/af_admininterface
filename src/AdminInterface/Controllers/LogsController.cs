using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers.Filters;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Transform;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Secure
	]
	public class LogsController : AdminInterfaceController
	{
		public LogsController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Documents([ARDataBind("filter", AutoLoadBehavior.NullIfInvalidKey)] DocumentFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["logEntities"] = filter.Find();
		}

		public void ShowUpdateDetails(uint updateLogEntityId)
		{
			CancelLayout();

			var logEntity = UpdateLogEntity.Find(updateLogEntityId);
			var detailsLogEntities = logEntity.UpdateDownload;
			var detailDocumentLogs = logEntity.GetLoadedDocumentLogs();

			ulong totalByteDownloaded = 0;
			ulong totalBytes = 1;
			foreach (var entity in detailsLogEntities)
			{
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

			var documentLog = DocumentReceiveLog.Find(documentLogId);
			PropertyBag["documentLogId"] = documentLogId;
			PropertyBag["documentLog"] = documentLog;
		}

		public void ShowDownloadLog(uint updateLogEntityId)
		{
			CancelLayout();

			PropertyBag["updateLogEnriryId"] = updateLogEntityId;
			PropertyBag["log"] = UpdateLogEntity.Find(updateLogEntityId).Log;
		}

		public void UpdateLog(UpdateType? updateType, ulong regionMask, uint? clientCode, uint? userId)
		{
			UpdateLog(updateType, regionMask, clientCode, userId, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void UpdateLog(UpdateType? updateType, ulong regionMask, uint? clientCode, uint? userId,
			DateTime beginDate, DateTime endDate)
		{
			var filter = new UpdateFilter();
			filter.BeginDate = beginDate;
			filter.EndDate = endDate;

			if (updateType.HasValue)
			{
				filter.UpdateType = updateType;
				filter.RegionMask = regionMask & Admin.RegionMask;
			}
			if (clientCode.HasValue)
				filter.Client = Client.Find(clientCode.Value);
			else if (userId.HasValue)
				filter.User = User.Find(userId.Value);

			PropertyBag["beginDate"] = filter.BeginDate;
			PropertyBag["endDate"] = filter.EndDate;
			PropertyBag["filter"] = filter;
			PropertyBag["logEntities"] = filter.Find().SortBy(Request["SortBy"], Request["Direction"] == "desc");
			PropertyBag["SortBy"] = Request["SortBy"];
			PropertyBag["Direction"] = Request["Direction"];
		}

		public void PasswordChangeLog(uint id)
		{
			PasswordChangeLog(id, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void PasswordChangeLog(uint id, DateTime beginDate, DateTime endDate)
		{
			var user = User.Find(id);

			PropertyBag["logEntities"] = PasswordChangeLogEntity.GetByLogin(user.Login,
				beginDate,
				endDate.AddDays(1));
			PropertyBag["login"] = user.Login;
			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}

		public void Orders([ARDataBind("filter", AutoLoadBehavior.NullIfInvalidKey)] OrderFilter filter)
		{
			if (filter.Client == null && filter.User != null)
				filter.Client = filter.User.Client;

			PropertyBag["orders"] = OrderLog.Load(filter);
			PropertyBag["filter"] = filter;
		}
	}

	public class OrderFilter
	{
		public Client Client { get; set; }
		public User User { get; set; }
		public Address Address { get; set; }
		public Supplier Supplier { get; set; }
		public DatePeriod Period { get; set; }

		public IList<User> Users
		{
			get { return Client.Users.OrderBy(u => u.GetLoginOrName()).ToList(); }
		}

		public IList<Address> Addresses
		{
			get { return Client.Addresses.OrderBy(u => u.Name).ToList(); }
		}

		public OrderFilter()
		{
			Period = new DatePeriod{
				Begin = DateTime.Today,
				End = DateTime.Today
			};
		}
	}

	public class UpdateFilter : SortableContributor
	{
		public UpdateType? UpdateType { get; set; }
		public ulong RegionMask { get; set; }
		public DateTime BeginDate { get; set; }
		public DateTime EndDate { get; set; }
		public Client Client { get; set; }
		public User User { get; set; }

		public IList<UpdateLogEntity> Find()
		{
			if (User != null)
				return UpdateLogEntity.GetEntitiesByUser(User.Id, BeginDate, EndDate);
			if (Client != null)
				return UpdateLogEntity.GetEntitiesFormClient(Client.Id, BeginDate, EndDate);

			return UpdateLogEntity.GetEntitiesByUpdateType(UpdateType, RegionMask, BeginDate, EndDate);
		}

		public bool ShowRegion()
		{
			return UpdateType != null;
		}

		public bool ShowClient()
		{
			return ShowRegion();
		}

		public bool ShowUpdateType()
		{
			return User != null || Client != null;
		}

		private bool IsDataTransferUpdate()
		{
			return UpdateType != null
				&& (UpdateType == Models.Logs.UpdateType.Cumulative
					|| UpdateType == Models.Logs.UpdateType.Accumulative);
		}

		public bool ShowUser()
		{
			return Client != null || UpdateType != null;
		}

		public bool ShowUpdateSize()
		{
			return ShowUpdateType();
		}

		public bool ShowLog()
		{
			return ShowUpdateType();
		}

		public string GetUri()
		{
			var result = new StringBuilder();
			if (UpdateType != null)
			{
				result.Append("updateType=" + (int)UpdateType);
				result.Append("&regionMask=" + RegionMask);
			}
			if (Client != null)
				result.Append("clientcode=" + Client.Id);
			if (User != null)
				result.Append("userid=" + User.Id);

			result.Append("&beginDate="+BeginDate.ToShortDateString());
			result.Append("&endDate="+EndDate.ToShortDateString());

			return result.ToString();
		}
	}

	public class OrderLog
	{
		public uint Id { get; set; }
		public uint? ClientOrderId { get; set; }
		public DateTime WriteTime { get; set; }
		public DateTime PriceDate { get; set; }

		public uint AddressId { get; set; }
		public uint UserId { get; set; }

		public string Drugstore { get; set; }
		public string Address { get; set; }
		public string User { get; set; }

		public string Supplier { get; set; }
		public string PriceName { get; set; }

		public uint PriceId { get; set; }

		public uint RowCount { get; set; }
		public double Sum { get; set; }
		public uint? ResultCode { get; set; }
		public uint? TransportType { get; set; }

		public uint SmtpId { get; set; }

		public string GetResult()
		{
			if (TransportType == null || ResultCode == 0)
				return "Не отправлен";

			if (PriceId == 2647)
				return "ok (Обезличенный заказ)";

			switch (TransportType)
			{
				case 1:
					return ResultCode.ToString();
				case 2:
					return "ok (Ftp Инфорум)";
				case 4:
					return "ok (Ftp Поставщика)";
				default:
					return "ok (Собственный отправщик)";
			}
		}

		public static IList<OrderLog> Load(OrderFilter filter)
		{
			return ArHelper.WithSession(s => {

				var sqlFilter = "(oh.writetime >= :FromDate AND oh.writetime <= ADDDATE(:ToDate, INTERVAL 1 DAY))";
				if (filter.User != null)
					sqlFilter += "and oh.UserId = :UserId ";

				if (filter.Address != null)
					sqlFilter += "and oh.AddressId = :AddressId ";

				if (filter.Client != null)
					sqlFilter += "and oh.ClientCode = :ClientId ";

				if (filter.Supplier != null)
					sqlFilter += "and pd.FirmCode = :SupplierId";

				var query = s.CreateSQLQuery(String.Format(@"
SELECT  oh.rowid as Id,
		oh.WriteTime,
		oh.PriceDate,
		c.Name as Drugstore,
		a.Address,
		a.Id as AddressId,
		u.Id as UserId,
		if (u.Name is not null and length(u.Name) > 0, u.Name, u.Login) as User,
		s.Name as Supplier,
		pd.PriceName,
		pd.PriceCode PriceId,
		oh.RowCount,
		max(o.ResultCode) as ResultCode,
		o.TransportType,
		oh.ClientOrderId
FROM orders.ordershead oh
	join usersettings.pricesdata pd on pd.pricecode = oh.pricecode
	join Future.Suppliers as s on s.Id = pd.firmcode
	join Future.Clients c on oh.ClientCode = c.Id
	join Future.Users u on u.Id = oh.UserId
	join Future.Addresses a on a.Id = oh.AddressId
		left join logs.orders o on oh.rowid = o.orderid
WHERE {0} and oh.RegionCode & :RegionCode > 0
	and oh.Deleted = 0
group by oh.rowid
ORDER BY writetime desc;", sqlFilter))
					.SetParameter("FromDate", filter.Period.Begin)
					.SetParameter("ToDate", filter.Period.End)
					.SetParameter("RegionCode", SecurityContext.Administrator.RegionMask);

				if (filter.User != null)
					query.SetParameter("UserId", filter.User.Id);

				if (filter.Address != null)
					query.SetParameter("AddressId", filter.Address.Id);

				if (filter.Client != null)
					query.SetParameter("ClientId", filter.Client.Id);

				if (filter.Supplier != null)
					query.SetParameter("SupplierId", filter.Supplier.Id);

				return query.ToList<OrderLog>();
			});
		}
	}
}