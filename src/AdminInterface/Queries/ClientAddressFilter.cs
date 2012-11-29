using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class AddressLogs
	{
		public uint Id { get; set; }
		public int Count { get; set; }
	}

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
		[Description("Клиент")]
		public string ClientText { get; set; }

		public ClientAddressFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "AddressId", "AddressId" },
				{ "AddressName", "AddressName" },
				{ "ClientId", "ClientId" },
				{ "ClientName", "ClientName" },
				{ "SupplierId", "SupplierId" },
				{ "Region", "c.HomeRegion" },
				{ "SupplierName", "SupplierName" },
				{ "Count", "Count" }
			};
			SortBy = "ClientName";
			PageSize = 100;
			Period = new DatePeriod(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
		}

		protected virtual DetachedCriteria GetCriteria()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var criteria = DetachedCriteria.For<RejectWaybillLog>();

			criteria.CreateCriteria("ForClient", "c", JoinType.LeftOuterJoin)
				.Add(Expression.Sql("{alias}.RegionCode & " + regionMask + " > 0"))
				.CreateAlias("HomeRegion", "r", JoinType.LeftOuterJoin);
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
				.Add(Projections.Property("r.Name").As("RegionName"))
				.Add(Projections.Property("f.Name").As("SupplierName")));
			criteria.Add(Expression.Ge("LogTime", Period.Begin.Date))
				.Add(Expression.Le("LogTime", Period.End));
			if (!string.IsNullOrEmpty(ClientText))
				criteria.Add(Expression.Like("c.Name", ClientText, MatchMode.Anywhere));
			return criteria;
		}

		public IList<RejectCounts> Find(ISession session)
		{
			var criteria = GetCriteria();
			var result = AcceptPaginator<RejectCounts>(criteria, session);

			return result;
		}
	}
}