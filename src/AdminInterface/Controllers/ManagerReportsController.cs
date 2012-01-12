using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Controllers
{
	public enum RegistrationFinderType
	{
		Users,
		Adresses
	}

	public class RegistrationInformation
	{
		public uint Id { get; set; }
		public string Name { get; set; }
		public DateTime RegistrationDate { get; set; }
		public bool AdressEnabled { get; set; }
		public bool ClientEnabled { get; set; }
		public bool UserEnabled { get; set; }
		public bool ServiceDisabled { get; set; }
	}

	public class RegistrationFinderTypeProperties : ICompisite
	{
		public RegistrationFinderType Value { get; set; }

		public bool IsUsers()
		{
			return Value == RegistrationFinderType.Users;
		}

		public bool IsAdresses()
		{
			return Value == RegistrationFinderType.Adresses;
		}

		public IEnumerable<Tuple<string, object>> Elements()
		{
			return new[] {new Tuple<string, object>("Value", Value)};
		}
	}

	public class UserFinderFilter : Sortable, IPaginable
	{
		public long region { get; set; }
		public RegistrationFinderTypeProperties finderType { get; set; }
		public DatePeriod Period { get; set; }

		private int _lastRowsCount;

		public int RowsCount
		{
			get { return _lastRowsCount; }
		}

		public int PageSize
		{
			get { return 30; }
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
				{"filter.Period.Begin", Period.Begin},
				{"filter.Period.End", Period.End},
				{"filter.region", region},
				{"filter.finderType.Value", finderType.Value},
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

		private string GetFieldToSort(string originalField)
		{
			if (finderType.Value == RegistrationFinderType.Adresses) {
				if (originalField == "Name")
					return "Address";
			}
			return originalField;
		}

		public IList<RegistrationInformation> Find()
		{
			return ArHelper.WithSession<IList<RegistrationInformation>>(s => {
				var newSql = string.Empty;
				if (finderType.Value == RegistrationFinderType.Users)
					newSql = string.Format(
						@"
SELECT u.Id, u.Name, u.RegistrationDate, u.Enabled as UserEnabled, s.Disabled as ServiceDisabled
FROM future.users u
join future.services s on s.id = u.rootservice
where
s.HomeRegion & :region > 0
and RegistrationDate >= :startDate
and RegistrationDate <= :endDate
and u.PayerId <> 921
ORDER BY {2}
limit {0}, {1}", CurrentPage*PageSize, PageSize, (string.IsNullOrEmpty(SortBy) ? "u.Id" : "u." + GetFieldToSort(SortBy)) + " " + SortDirection);

				if (finderType.Value == RegistrationFinderType.Adresses)
					newSql = string.Format(
						@"
SELECT a.Id, a.Address as Name, a.RegistrationDate, a.Enabled as AdressEnabled, c.Status as ClientEnabled
FROM future.addresses a
join future.Clients c on a.ClientId = c.id
where
c.RegionCode & :region > 0
and a.RegistrationDate >= :startDate
and a.RegistrationDate <= :endDate
ORDER BY {2}
limit {0}, {1}", CurrentPage*PageSize, PageSize, (string.IsNullOrEmpty(SortBy) ? "a.Id" : "a." + GetFieldToSort(SortBy)) + " " + SortDirection);

				var result = s.CreateSQLQuery(newSql)
						.SetParameter("region", region)
						.SetParameter("startDate", Period.Begin)
						.SetParameter("endDate", Period.End)
						.ToList<RegistrationInformation>();

				newSql = newSql.Remove(newSql.IndexOf("limit"));
				newSql = string.Format("select count(*) from ({0}) as t1;", newSql);
				var countQuery = s.CreateSQLQuery(newSql);
				countQuery.SetParameter("region", region)
					.SetParameter("startDate", Period.Begin)
					.SetParameter("endDate", Period.End);
				_lastRowsCount = Convert.ToInt32(countQuery.UniqueResult());

				return result;
			});
		}
	}


	[
		Helper(typeof(PaginatorHelper), "paginator"),
		Secure(PermissionType.ManagerReport),
	]
	public class ManagerReportsController: ARSmartDispatcherController
	{
		public void UsersAndAdresses([DataBind("filter")]UserFinderFilter userFilter)
		{
			PropertyBag["Regions"] = RegionHelper.GetAllRegions();
			PropertyBag["chRegion"] = userFilter.region;

			if (userFilter.Period == null)
				userFilter.Period = new DatePeriod(DateTime.Now.AddDays(-1), DateTime.Now);
			if (userFilter.finderType == null)
				userFilter.finderType = new RegistrationFinderTypeProperties {Value = RegistrationFinderType.Users};

			PropertyBag["filter"] = userFilter;

			PropertyBag["Users"] = userFilter.Find();
		}
	}
}