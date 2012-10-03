using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class SwitchOffCounts : BaseLogsQueryFields
	{
		public string LogTime { get; set; }
	}

	public class SwitchOffClientsFilter : PaginableSortable
	{
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }

		public SwitchOffClientsFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientId", "c.Id" },
				{ "ClientName", "c.Name" }
			};
			PageSize = 30;
			Period = new DatePeriod(DateTime.Now.AddMonths(-1), DateTime.Now);
		}

		protected DetachedCriteria GetCriteria()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var criteria = DetachedCriteria.For<ClientLogRecord>();

			criteria.CreateCriteria("Client", "c", JoinType.LeftOuterJoin)
				.Add(Expression.Sql("{alias}.RegionCode & " + regionMask + " > 0"));

			criteria.SetProjection(Projections.ProjectionList()
				.Add(Projections.Max("LogTime").As("LogTime"))
				.Add(Projections.GroupProperty("c.Id").As("ClientId"))
				.Add(Projections.Property("c.Name").As("ClientName"))
				.Add(Projections.Property("c.HomeRegion").As("RegionName")));
			criteria.Add(Expression.Ge("LogTime", Period.Begin.Date))
				.Add(Expression.Le("LogTime", Period.End.Date))
				.Add(Expression.Eq("c.Status", ClientStatus.Off));
			return criteria;
		}

		public IList<SwitchOffCounts> Find(ISession session)
		{
			var criteria = GetCriteria();
			var result = AcceptPaginator<SwitchOffCounts>(criteria, session);
			return result;
		}
	}
}