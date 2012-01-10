using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	public class RequestFilter : Sortable, IPaginable
	{
		public DateTime? startDate { get; set; }
		public DateTime? endDate { get; set; }
		public long region { get; set; }
		//public string Direction { get; set; }
		//public string SortBy { get; set; }

		private int _lastRowsCount;

		public int RowsCount
		{
			get { return _lastRowsCount; }
		}

		public int PageSize
		{
			get { return 20; }
		}

		public int CurrentPage { get; set; }

		public string[] ToUrl()
		{
			return
				GetParameters().Where(p => p.Key != "CurrentPage").Select(p => string.Format("{0}={1}", p.Key, p.Value))
					.ToArray();
		}

		public Dictionary<string, object> GetParameters()
		{
			return new Dictionary<string, object> {
				{"filter.startDate", startDate},
				{"filter.endDate", endDate},
				{"filter.region", region},
				{"filter.Direction", SortDirection},
				{"filter.SortBy", SortBy}

			};
		}

		public string ToUrlQuery()
		{
			return string.Join("&", ToUrl());
		}


		public string GetUri()
		{
			return ToUrlQuery();
		}

		public IList<User> Find()
		{
			var users = ArHelper.WithSession(s => {
				var newSql =
					string.Format(
						@"
SELECT u.* FROM future.users u
join future.services s on s.id = u.rootservice
where
s.HomeRegion & :region > 0
and RegistrationDate >= :startDate
and RegistrationDate <= :endDate
ORDER BY {2}
limit {0}, {1}",
						CurrentPage*PageSize, PageSize, (string.IsNullOrEmpty(SortBy) ? "u.Id" : "u." + SortBy) + " " + SortDirection);
				var result =
					s.CreateSQLQuery(newSql)
						.AddEntity(typeof (User))
						.SetParameter("region", region)
						.SetParameter("startDate", startDate)
						.SetParameter("endDate", endDate)
						.List<User>();

				newSql = newSql.Remove(newSql.IndexOf("limit"));
				newSql = string.Format("select count(*) from ({0}) as t1;", newSql);
				var countQuery = s.CreateSQLQuery(newSql);
				countQuery.SetParameter("region", region)
					.SetParameter("startDate", startDate)
					.SetParameter("endDate", endDate);
				_lastRowsCount = Convert.ToInt32(countQuery.UniqueResult());

				return result;
			});
			return users;
		}
	}

	[
		Helper(typeof(PaginatorHelper), "paginator"),
		Secure,
	]
	public class ManagerReportsController: ARSmartDispatcherController
	{
		public void UsersAndAdresses([DataBind("filter")]RequestFilter filter)
		{
			PropertyBag["Regions"] = RegionHelper.GetAllRegions();
			PropertyBag["chRegion"] = filter.region;

			if (filter.startDate == null)
				filter.startDate = DateTime.Now.AddDays(-1);
			if (filter.endDate == null)
				filter.endDate = DateTime.Now;

			PropertyBag["startDate"] = filter.startDate.Value.ToString("yyyy-MM-dd");
			PropertyBag["endDate"] = filter.endDate.Value.ToString("yyyy-MM-dd");

			PropertyBag["filter"] = filter;

			PropertyBag["Users"] = filter.Find();
		}
	}
}