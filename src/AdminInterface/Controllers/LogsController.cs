using System;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Transform;

namespace AdminInterface.Controllers
{
	[
		Layout("logs"),
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Secure
	]
	public class LogsController : SmartDispatcherController
	{
		public void DocumentLog(uint? clientCode, uint? userId)
		{
			DocumentLog(clientCode, userId, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void DocumentLog(uint? clientCode, uint? userId, DateTime beginDate, DateTime endDate)
		{
			if (!userId.HasValue)
			{
				var client = Client.Find(clientCode.Value);

				SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
				SecurityContext.Administrator.CheckClientType(client.Type);

				PropertyBag["logEntities"] = DocumentLogEntity.GetEnitiesForClient(client,
					beginDate, endDate.AddDays(1));
				PropertyBag["client"] = client;
			}
			else
			{
				var user = User.Find(userId.Value);
				Client.FindAndCheck(user.Client.Id);
				PropertyBag["user"] = user;
				PropertyBag["logEntities"] = DocumentLogEntity.GetEnitiesForUser(user,
					beginDate, endDate.AddDays(1));
			}

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

		public void UpdateLog(uint? clientCode, uint? userId)
		{
			UpdateLog(clientCode, userId, DateTime.Today.AddDays(-1), DateTime.Today);
		}

		public void UpdateLog(uint? clientCode, uint? userId, DateTime beginDate, DateTime endDate)
		{

			if (!userId.HasValue)
			{
				var client = Client.Find(clientCode.Value);
				PropertyBag["client"] = client;
				PropertyBag["logEntities"] = UpdateLogEntity.GetEntitiesFormClient(client.Id,
					beginDate, endDate.AddDays(1));
				SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
				SecurityContext.Administrator.CheckClientType(client.Type);
			}
			else
			{
				var user = User.Find(userId.Value);
				PropertyBag["user"] = user;
				PropertyBag["logEntities"] = UpdateLogEntity.GetEntitiesByUser(userId.Value,
					beginDate, endDate.AddDays(1));
				SecurityContext.Administrator.CheckClientHomeRegion(user.Client.HomeRegion.Id);
				SecurityContext.Administrator.CheckClientType(user.Client.Type);
			}
			
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

		public void Orders(uint? clientId, uint? userId)
		{
			Orders(clientId, userId, DateTime.Today, DateTime.Today);
		}

		public void Orders(uint? clientId, uint? userId, DateTime beginDate, DateTime endDate)
		{
			if (!userId.HasValue)
			{
				PropertyBag["client"] = Client.FindAndCheck(clientId.Value);
				PropertyBag["Orders"] = OrderLog.Load(clientId.Value, beginDate, endDate);
			}
			else
			{
				var user = User.Find(userId.Value);
				Client.FindAndCheck(user.Client.Id);
				PropertyBag["user"] = user;
				PropertyBag["Orders"] = OrderLog.LoadByUser(userId.Value, beginDate, endDate);
			}

			PropertyBag["beginDate"] = beginDate;
			PropertyBag["endDate"] = endDate;
		}
	}

	public class OrderLog
	{
		public uint Id { get; set; }
		public uint? ClientOrderId { get; set; }
		public DateTime WriteTime { get; set; }
		public DateTime PriceDate { get; set; }

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

		public static IList<OrderLog> Load(uint clientId, DateTime begin, DateTime end)
		{
			return ArHelper.WithSession(s => s.CreateSQLQuery(@"
SELECT  oh.rowid as Id,
		oh.WriteTime,
		oh.PriceDate,
		c.Name as Drugstore,
		a.Address,
		u.Login as User,
		firm.shortname as Supplier,
		pd.PriceName,
		pd.PriceCode PriceId,
		oh.RowCount,
		max(o.ResultCode) as ResultCode,
		o.TransportType,
		oh.ClientOrderId
FROM orders.ordershead oh
	join usersettings.pricesdata pd on pd.pricecode = oh.pricecode
	join usersettings.clientsdata as firm on firm.firmcode = pd.firmcode
	join Future.Clients c on oh.ClientCode = c.Id
	join Future.Users u on u.Id = oh.UserId
	join Future.Addresses a on a.Id = oh.AddressId
		left join logs.orders o on oh.rowid = o.orderid
WHERE oh.writetime BETWEEN :FromDate AND ADDDATE(:ToDate, INTERVAL 1 DAY)
	AND oh.ClientCode = :ClientId
	AND oh.RegionCode & :RegionCode > 0
	and oh.Deleted = 0
group by oh.rowid
ORDER BY writetime desc;")
				.SetParameter("FromDate", begin)
				.SetParameter("ToDate", end)
				.SetParameter("RegionCode", SecurityContext.Administrator.RegionMask)
				.SetParameter("ClientId", clientId)
				.SetResultTransformer(Transformers.AliasToBean<OrderLog>())
				.List<OrderLog>());
		}

		public static IList<OrderLog> LoadByUser(uint userId, DateTime begin, DateTime end)
		{
			return ArHelper.WithSession(s => s.CreateSQLQuery(@"
SELECT  oh.rowid as Id,
		oh.WriteTime,
		oh.PriceDate,
		c.Name as Drugstore,
		a.Address,
		u.Login as User,
		firm.shortname as Supplier,
		pd.PriceName,
		pd.PriceCode PriceId,
		oh.RowCount,
		max(o.ResultCode) as ResultCode,
		o.TransportType,
		oh.ClientOrderId
FROM orders.ordershead oh
	join usersettings.pricesdata pd on pd.pricecode = oh.pricecode
	join usersettings.clientsdata as firm on firm.firmcode = pd.firmcode
	join Future.Clients c on oh.ClientCode = c.Id
	join Future.Users u on u.Id = oh.UserId
	join Future.Addresses a on a.Id = oh.AddressId
		left join logs.orders o on oh.rowid = o.orderid
WHERE oh.writetime BETWEEN :FromDate AND ADDDATE(:ToDate, INTERVAL 1 DAY)
	AND oh.UserId = :UserId
	AND oh.RegionCode & :RegionCode > 0
	and oh.Deleted = 0
group by oh.rowid
ORDER BY writetime desc;")
				.SetParameter("FromDate", begin)
				.SetParameter("ToDate", end)
				.SetParameter("RegionCode", SecurityContext.Administrator.RegionMask)
				.SetParameter("UserId", userId)
				.SetResultTransformer(Transformers.AliasToBean<OrderLog>())
				.List<OrderLog>());
		}
	}
}