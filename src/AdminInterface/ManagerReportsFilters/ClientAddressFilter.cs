using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class BaseLogsQueryFields
	{
		public uint AddressId { get; set; }
		public uint ClientId { get; set; }
		public string ClientName { get; set; }
		public string AddressName { get; set; }
		public bool AddressEnabled { get; set; }
		public string RegionName { get; set; }
	}

	public class RejectCounts : BaseLogsQueryFields
	{
		public int Count { get; set; }
		public bool IsUpdate { get; set; }
		public uint SupplierId { get; set; }
		public string SupplierName { get; set; }
		public ClientStatus ClientStatus { get; set; }
		public RejectReasonType? RejectReason { get; set; }
		public string RejectReasonName
		{
			get
			{
				return RejectReason == null ? "" : RejectReason.Value.GetDescription();
			}
		}
	}

	public class ClientAddressFilter : PaginableSortable
	{
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }
		public RegistrationFinderType FinderType { get; set; }

		public ClientAddressFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "AddressId", "AddressId" },
				{ "AddressName", "AddressName" },
				{ "ClientId", "ClientId" },
				{ "ClientName", "ClientName" },
				{ "SupplierId", "SupplierId" },
				{ "SupplierName", "SupplierName" }
			};
			PageSize = 30;
			Period = new DatePeriod(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
		}

		protected virtual DetachedCriteria GetCriteria()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var criteria = DetachedCriteria.For<RejectWaybillLog>();

			criteria.CreateCriteria("ForClient", "c", JoinType.LeftOuterJoin)
				.Add(Expression.Sql("{alias}.RegionCode & " + regionMask + " > 0"));
			criteria.CreateAlias("Address", "a", JoinType.LeftOuterJoin);
			criteria.CreateAlias("FromSupplier", "f", JoinType.LeftOuterJoin);

			criteria.SetProjection(Projections.ProjectionList()
				.Add(Projections.Count("Id").As("Count"))
				.Add(Projections.GroupProperty("c.Id").As("ClientId"))
				.Add(Projections.GroupProperty("a.Id").As("AddressId"))
				.Add(Projections.GroupProperty("f.Id").As("SupplierId"))
				.Add(Projections.GroupProperty("RejectReason").As("RejectReason"))
				.Add(Projections.Property("c.Name").As("ClientName"))
				.Add(Projections.Property("a.Value").As("AddressName"))
				.Add(Projections.Property("c.HomeRegion").As("RegionName"))
				.Add(Projections.Property("f.Name").As("SupplierName")));
			criteria.Add(Expression.Ge("LogTime", Period.Begin.Date))
				.Add(Expression.Le("LogTime", Period.End.Date));
			return criteria;
		}

		public List<Address> Sort(List<Address> list)
		{
			var sortProperty = GetSortProperty();
			switch (sortProperty) {
				case "Id":
					if (SortDirection == "asc")
						return list.OrderBy(t => t.Id).ToList();
					return list.OrderByDescending(t => t.Id).ToList();
				case "c.Id":
					if (SortDirection == "asc")
						return list.OrderBy(t => t.Client.Id).ToList();
					return list.OrderByDescending(t => t.Client.Id).ToList();
				case "Name":
					if (SortDirection == "asc")
						return list.OrderBy(t => t.Value).ToList();
					return list.OrderByDescending(t => t.Value).ToList();
				case "c.Name":
					if (SortDirection == "asc")
						return list.OrderBy(t => t.Client.Name).ToList();
					return list.OrderByDescending(t => t.Client.Name).ToList();
			}
			return new List<Address>();
		}

		public IList<RejectCounts> Find()
		{
			var criteria = GetCriteria();
			var result = ArHelper.WithSession(s => AcceptPaginator<RejectCounts>(criteria, s));
			ArHelper.WithSession(s => {
				foreach (var row in result) {
					var address = s.Query<Address>().FirstOrDefault(a => a.Id == row.AddressId);
					if (address != null)
						row.IsUpdate = address.AvaliableForUsers.Count(u => u.Logs.AFTime >= DateTime.Now.AddMonths(-1)) != 0;
				}
			});
			return result;
		}
	}
}