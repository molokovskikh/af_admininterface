using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Controllers.Filters
{
	public class StatResult
	{
		public string ClientName { get; set; }
		public string SupplierName { get; set; }
		public string RequestTime { get; set; }
		public string ProductName { get; set; }
		public uint? ProductId { get; set; }

		public string GetReason()
		{
			if (ProductId.HasValue)
				return "Не найден файл.";
			return "Нет синонима.";
		}
	}

	public class StatisticsFilter : Sortable
	{
		public DatePeriod Period { get; set; }

		public StatisticsFilter()
		{
			SortBy = "c.Name";
			SortDirection = "Desc";
			SortKeyMap = new Dictionary<string, string> {
				{"ClientName", "c.Name"},
				{"SupplierName", "s.FullName"},
				{"RequestTime", "u.RequestTime"},
				{"ProductName", "db.Product"},
				{"ProductId", "db.ProductId"}
			};
		}

		public IList<StatResult> Find()
		{
			var sql = string.Format(@"
select 
c.Name as ClientName,
s.FullName as SupplierName,
u.RequestTime,
db.Product as ProductName,
db.ProductId
from Logs.CertificateRequestLogs l
	join Logs.AnalitFUpdates u on u.UpdateId = l.UpdateId
	join Customers.Users fu on fu.Id = u.UserId
	join Customers.Clients c on c.Id = fu.ClientId
	join Documents.DocumentBodies db on db.Id = l.DocumentBodyId
	join Documents.DocumentHeaders dh on dh.Id = db.DocumentId
	join Customers.Suppliers s on s.Id = dh.FirmCode
where (u.RequestTime >= :StartDateParam AND u.RequestTime <= :EndDateParam)
	and c.MaskRegion & :RegionMaskParam > 0
	and fu.PayerId <> 921 and
l.CertificateId is null
order by {0} {1}
;", SortBy, SortDirection);
			var result = ArHelper.WithSession(s => s.CreateSQLQuery(sql)
				.SetParameter("StartDateParam", Period.Begin)
				.SetParameter("EndDateParam", Period.End.AddDays(1))
				.SetParameter("RegionMaskParam", SecurityContext.Administrator.RegionMask)
				.ToList<StatResult>());

			return result;
		}
	}
}