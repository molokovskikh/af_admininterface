using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminInterface.Security;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.Models;
using Dapper;
using NHibernate;

namespace AdminInterface.ViewModels.Reports
{
	public class WaybillStatisticsFilter : ATableFilter
	{
		public WaybillStatisticsFilter() : base()
		{
			DateBegin = SystemTime.Now().Date.AddDays(-6);
		}

		public ulong ClientId { get; set; }
	}

	public class WaybillStatisticsData
	{
		public uint SupplierId { get; set; }
		public string SupplierName { get; set; }
		public uint ClientId { get; set; }
		public string ClientName { get; set; }
		public int SupplierOrdersNumber { get; set; }
		public int WaybillsTotalNumber { get; set; }
		public int WaybillsParsedNumber { get; set; }

		private static string[] _sortOrder = new[] {
			"SupplierName", "ClientName", "SupplierOrdersNumber", "WaybillsTotalNumber", "WaybillsParsedNumber"
		};

		public static int GetReportTotalItemsNumber(ISession dbSession,
			ReportTable<WaybillStatisticsFilter, WaybillStatisticsData> reportTable)
		{
			var sqlString = @"
SELECT  Count(*) FROM (
SELECT
  DISTINCT  oh.clientcode as ClientId, c.Name ClientName, pd.FirmCode as SupplierId, s.Name SupplierName,
	COUNT(DISTINCT oh.rowid) SupplierOrdersNumber,
	COUNT(DISTINCT dl.rowid) WaybillsTotalNumber,
	COUNT(DISTINCT dh.id) WaybillsParsedNumber,
	COUNT(DISTINCT IF(db.ProductId IS NULL,db.DocumentId, NULL))notwhellknown
FROM	(orders.ordershead oh,
	usersettings.pricesdata pd,
	customers.Suppliers s,
	customers.clients c)
LEFT JOIN  logs.Document_Logs dl
 ON c.id=dl.ClientCode
 AND dl.DocumentType=1
 AND logtime>=@periodStart
 AND oh.writetime<@periodFinish
 AND pd.firmcode=dl.FirmCode
LEFT JOIN documents.documentheaders dh
ON dh.DownloadId=dl.rowid AND dh.ClientCode=oh.ClientCode
 LEFT JOIN documents.documentbodies db
 ON db.DocumentId=dh.Id
WHERE
	c.Id = @ClientId
	AND pd.firmcode=s.id
  AND oh.pricecode=pd.PriceCode
  AND oh.writetime>=@periodStart
  AND oh.writetime<@periodFinish
	AND c.id=oh.ClientCode
GROUP BY pd.firmcode, oh.ClientCode
) as tempList
";

			return dbSession.Connection.QueryFirst<int>(sqlString,
				new {
					@clientId = reportTable.TableFilter.ClientId,
					@periodStart = reportTable.TableFilter.DateBegin.Date,
					@periodFinish = reportTable.TableFilter.DateEnd.Date.AddDays(1),
				});
		}

		public static List<WaybillStatisticsData> GetReportData(ISession dbSession,
			ReportTable<WaybillStatisticsFilter, WaybillStatisticsData> reportTable)
		{
			var sqlString = @"

SELECT
  DISTINCT  oh.clientcode as ClientId, c.Name ClientName, pd.FirmCode as SupplierId, s.Name SupplierName,
	COUNT(DISTINCT oh.rowid) SupplierOrdersNumber,
	COUNT(DISTINCT dl.rowid) WaybillsTotalNumber,
	COUNT(DISTINCT dh.id) WaybillsParsedNumber,
	COUNT(DISTINCT IF(db.ProductId IS NULL,db.DocumentId, NULL))notwhellknown
FROM	(orders.ordershead oh,
	usersettings.pricesdata pd,
	customers.Suppliers s,
	customers.clients c)
LEFT JOIN  logs.Document_Logs dl
 ON c.id=dl.ClientCode
 AND dl.DocumentType=1
 AND logtime>=@periodStart
 AND oh.writetime<@periodFinish
 AND pd.firmcode=dl.FirmCode
LEFT JOIN documents.documentheaders dh
ON dh.DownloadId=dl.rowid AND dh.ClientCode=oh.ClientCode
 LEFT JOIN documents.documentbodies db
 ON db.DocumentId=dh.Id
WHERE
	c.Id = @ClientId
	AND pd.firmcode=s.id
  AND oh.pricecode=pd.PriceCode
  AND oh.writetime>=@periodStart
  AND oh.writetime<@periodFinish
	AND c.id=oh.ClientCode
GROUP BY pd.firmcode, oh.ClientCode
{0}
{1}
";
			string orderby = String.Format("order by {0} {1}",
				_sortOrder[Math.Abs(reportTable.TableHead.SortOrder) - 1],
				(reportTable.TableHead.SortOrder > 0) ? "asc" : "desc");
			string limit = String.Format("limit {0}, {1}",
				reportTable.TablePaginator.CurrentPage*reportTable.TablePaginator.PageSize,
				reportTable.TablePaginator.PageSize);

			return
				dbSession.Connection.Query<WaybillStatisticsData>
					(String.Format(sqlString, orderby, limit),
						new {
							@clientId = reportTable.TableFilter.ClientId,
							@periodStart = reportTable.TableFilter.DateBegin.Date,
							@periodFinish = reportTable.TableFilter.DateEnd.Date.AddDays(1),
						}).ToList();
		}
	}
}