using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class SwitchOffCounts : BaseLogsQueryFields
	{
		public string LogTime { get; set; }
		public string Comment { get; set; }
	}

	public class SwitchOffClientsFilter : PaginableSortable
	{
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }

		public SwitchOffClientsFilter()
		{
			SortKeyMap = new Dictionary<string, string> {
				{ "ClientId", "c.Id" },
				{ "ClientName", "c.Name" },
				{ "HomeRegion", "c.HomeRegion" },
				{ "LogTime", "LogTime" }
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

			var logsRecord = result.Select(r =>
				session.Query<PayerAuditRecord>().Where(p =>
					p.ObjectType == LogObjectType.Client &&
						p.ObjectId == r.ClientId &&
						(p.Message.Contains("Изменено 'Включен' было 'Включен' стало 'Отключен'") ||
							p.Message.Contains("Изменено 'Включен' было 'вкл' стало 'откл'")))
					.ToList())
				.Select(c => c.OrderBy(l => l.WriteTime).LastOrDefault())
				.Where(r => r != null)
				.ToDictionary(c => c.ObjectId);


			result = result.Select(r => {
				if (logsRecord.Keys.Contains(r.ClientId))
					r.Comment = logsRecord[r.ClientId].Comment;
				return r;
			}).ToList();

			return result;
		}
	}
}