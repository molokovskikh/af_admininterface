using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class WhoWasNotUpdatedField : BaseLogsQueryFields
	{
		public string Registrant { get; set; }
		public string Updatedate { get; set; }
	}

	public class WhoWasNotUpdatedFilter : PaginableSortable
	{
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }

		public WhoWasNotUpdatedFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientId", "c.Id" },
				{ "ClientName", "c.Name" }
			};
			PageSize = 30;
			Period = new DatePeriod(DateTime.Now.AddMonths(-1), DateTime.Now);
		}

		public DetachedCriteria GetCriteria()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var criteria = DetachedCriteria.For<UserUpdateInfo>();

			criteria
				.CreateCriteria("User", "u", JoinType.LeftOuterJoin)
				.CreateCriteria("u.Client", "c", JoinType.LeftOuterJoin)
				.Add(Expression.Sql("{alias}.RegionCode & " + regionMask + " > 0"));

			criteria.SetProjection(Projections.ProjectionList()
				.Add(Projections.Property("c.Id").As("ClientId"))
				.Add(Projections.Property("c.Name").As("ClientName"))
				.Add(Projections.Property("c.HomeRegion").As("RegionName"))
				.Add(Projections.Property("c.Registration.Registrant").As("Registrant"))
				.Add(Projections.Property("UpdateDate").As("Updatedate")));
			criteria.Add(Expression.Ge("UpdateDate", Period.Begin.Date))
				.Add(Expression.Le("UpdateDate", Period.End.Date));
			return criteria;
		}

		public IList<WhoWasNotUpdatedField> Find(ISession session)
		{
			var criteria = GetCriteria();
			var query = AcceptPaginator<WhoWasNotUpdatedField>(criteria, session);
			return query;
		}
	}
}