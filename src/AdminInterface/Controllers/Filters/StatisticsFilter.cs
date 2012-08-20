using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Security;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Controllers.Filters
{
	public class StatResult
	{
		public uint ClientCode { get; set; }
		public uint UserId { get; set; }
		public string ClientName { get; set; }
		public string ClientRegion { get; set; }
		public string SupplierId { get; set; }
		public string SupplierName { get; set; }
		public string SupplierRegion { get; set; }
		public string RequestTime { get; set; }
		public string ProcuctCode { get; set; }
		public string ProductName { get; set; }
		public uint ProducerId { get; set; }
		public string Producer { get; set; }
		public uint? ProductId { get; set; }
		public string CertificateError { get; set; }

		public string GetReason()
		{
			if (ProductId == null)
				return "Нет синонима.";
			return CertificateError;
		}
	}

	public class StatisticsFilter : Sortable
	{
		public DatePeriod Period { get; set; }
		public Region Region { get; set; }

		public StatisticsFilter()
		{
			SortBy = "c.Name";
			SortDirection = "Desc";
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientCode", "c.id" },
				{ "UserId", "u.UserId" },
				{ "ClientName", "c.Name" },
				{ "ClientRegion", "r1.Region" },
				{ "SupplierName", "s.Name" },
				{ "SupplierRegion", "r2.Region" },
				{ "ProcuctCode", "db.Code" },
				{ "RequestTime", "u.RequestTime" },
				{ "ProductName", "db.Product" },
				{ "ProductId", "db.ProductId" },
				{ "Producer", "db.Producer" }
			};
		}

		public IList<StatResult> Find()
		{
			var sql = string.Format(@"
select 
c.id as ClientCode,
u.UserId,
c.Name as ClientName,
r1.Region as ClientRegion,
s.Name as SupplierName,
s.Id as SupplierId,
r2.Region as SupplierRegion,
u.RequestTime,
db.Code ProcuctCode,
db.Product as ProductName,
db.Producer,
db.ProductId,
db.CertificateError
from Logs.CertificateRequestLogs l
	join Logs.AnalitFUpdates u on u.UpdateId = l.UpdateId
	join Customers.Users fu on fu.Id = u.UserId
	join Customers.Clients c on c.Id = fu.ClientId
	join Documents.DocumentBodies db on db.Id = l.DocumentBodyId
	join Documents.DocumentHeaders dh on dh.Id = db.DocumentId
	join Customers.Suppliers s on s.Id = dh.FirmCode
	join farm.Regions r1 on r1.RegionCode = c.RegionCode
	join farm.Regions r2 on r2.RegionCode = s.HomeRegion
where (u.RequestTime >= :StartDateParam AND u.RequestTime <= :EndDateParam)
	and c.MaskRegion & :RegionMaskParam > 0
	and fu.PayerId <> 921 and
l.CertificateId is null
order by {0} {1}
;", SortBy, SortDirection);
			var adminMask = SecurityContext.Administrator.RegionMask;
			if (Region != null) {
				adminMask &= Region.Id;
			}
			var result = ArHelper.WithSession(s => s.CreateSQLQuery(sql)
				.SetParameter("StartDateParam", Period.Begin)
				.SetParameter("EndDateParam", Period.End.AddDays(1))
				.SetParameter("RegionMaskParam", adminMask)
				.ToList<StatResult>());

			return result;
		}
	}
}