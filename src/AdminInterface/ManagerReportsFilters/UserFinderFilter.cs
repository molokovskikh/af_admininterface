﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Security;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.ManagerReportsFilters
{
	public class UserFinderFilter : Sortable, IPaginable
	{
		public Region Region { get; set; }
		public RegistrationFinderType FinderType { get; set; }
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

		public UserFinderFilter()
		{
			SortBy = "RegistrationDate";
			SortDirection = "Desc";

			SortKeyMap = new Dictionary<string, string> {
				{ "Id", "Id" },
				{ "Name", "Name" },
				{ "RegistrationDate", "Registration.RegistrationDate" },
				{ "ClientId", "c.Id" },
				{ "ClientName", "c.Name" }
			};

			Period = new DatePeriod(DateTime.Now.AddDays(-1), DateTime.Now);
			FinderType = RegistrationFinderType.Users;
		}

		public string HeadCodeName
		{
			get
			{
				if (FinderType == RegistrationFinderType.Users)
					return "Код пользователя";
				if (FinderType == RegistrationFinderType.Addresses)
					return "Код адреса";
				return string.Empty;
			}
		}

		public string HeadName
		{
			get
			{
				if (FinderType == RegistrationFinderType.Users)
					return "Комментарий к пользователю";
				if (FinderType == RegistrationFinderType.Addresses)
					return "Адрес";
				return string.Empty;
			}
		}

		public bool ShowUserNames()
		{
			return FinderType == RegistrationFinderType.Addresses;
		}

		private IList<RegistrationInformation> AcceptPaginator(DetachedCriteria criteria)
		{
			var countSubquery = CriteriaTransformer.TransformToRowCount(criteria);
			_lastRowsCount = ArHelper.WithSession(s => countSubquery.GetExecutableCriteria(s).UniqueResult<int>());

			if (CurrentPage > 0)
				criteria.SetFirstResult(CurrentPage * PageSize);

			criteria.SetMaxResults(PageSize);

			ApplySort(criteria);

			return ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).ToList<RegistrationInformation>()).ToList();
		}

		public DetachedCriteria GetCriteria()
		{
			if (FinderType == RegistrationFinderType.Users)
				SortKeyMap.Add("RegionName", "s.HomeRegion");
			if (FinderType == RegistrationFinderType.Addresses)
				SortKeyMap.Add("RegionName", "c.HomeRegion");

			var userCountProjection = Projections.SubQuery(DetachedCriteria.For<Client>()
				.CreateAlias("Users", "u", JoinType.InnerJoin)
				.Add(Expression.EqProperty("Id", "c.Id"))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Count("u.Id"))));

			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			if (FinderType == RegistrationFinderType.Users) {
				var userCriteria = DetachedCriteria.For<User>();

				userCriteria.CreateCriteria("RootService", "s", JoinType.InnerJoin)
					.Add(Expression.Sql("{alias}.HomeRegion & " + regionMask + " > 0"))
					.SetProjection(Projections.ProjectionList().Add(Projections.SqlProjection("{alias}.HomeRegion as RegionName", new[] { "RegionName" }, new[] { NHibernateUtil.String })));

				userCriteria.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property<User>(u => u.Id).As("Id"))
					.Add(Projections.Property<User>(u => u.Name).As("Name"))
					.Add(Projections.Property<User>(u => u.Registration.RegistrationDate).As("RegistrationDate"))
					.Add(Projections.Property("Enabled").As("UserEnabled"))
					.Add(Projections.Property("s.Id").As("ClientId"))
					.Add(Projections.Property("s.Name").As("ClientName"))
					.Add(Projections.Property("s.Disabled").As("ServiceDisabled"))
					.Add(Projections.Property("s.Type").As("ClientType"))
					.Add(Projections.Property("s.HomeRegion").As("RegionName"))
					.Add(Projections.Alias(userCountProjection, "UserCount")))
					.Add(Expression.Ge("Registration.RegistrationDate", Period.Begin.Date))
					.Add(Expression.Le("Registration.RegistrationDate", Period.End.Date))
					.CreateAlias("Payer", "p", JoinType.InnerJoin)
					.CreateAlias("Client", "c", JoinType.LeftOuterJoin)
					.Add(Expression.Or(Expression.Gt("p.PayerID", 921u), Expression.Lt("p.PayerID", 921u)));

				return userCriteria;
			}

			if (FinderType == RegistrationFinderType.Addresses) {
				var adressCriteria = DetachedCriteria.For<Address>("ad");

				adressCriteria.CreateCriteria("Client", "c", JoinType.InnerJoin)
					.Add(Expression.Sql("{alias}.RegionCode & " + regionMask + " > 0"));

				adressCriteria.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property<Address>(u => u.Id).As("Id"))
					.Add(Projections.Property<Address>(u => u.Value).As("Name"))
					.Add(Projections.Property<Address>(u => u.Registration.RegistrationDate).As("RegistrationDate"))
					.Add(Projections.Property("Enabled").As("AdressEnabled"))
					.Add(Projections.Property("c.Id").As("ClientId"))
					.Add(Projections.Property("c.Name").As("ClientName"))
					.Add(Projections.Property("c.Status").As("ClientEnabled"))
					.Add(Projections.Property("c.Type").As("ClientType"))
					.Add(Projections.Property("c.HomeRegion").As("RegionName"))
					.Add(Projections.Alias(userCountProjection, "UserCount"))
					.Add(Projections.Alias(Projections.SubQuery(
						DetachedCriteria.For<Client>()
							.Add(Expression.EqProperty("Id", "c.Id"))
							.Add(Expression.And(
								Expression.LeProperty(
									Projections.SqlFunction("DATE_ADD", NHibernateUtil.DateTime,
										Projections.Property("u.Registration.RegistrationDate"),
										Projections.SqlProjection("INTERVAL -1 hour as ",
											new[] { string.Empty },
											new[] { NHibernateUtil.DateTime })),
									"ad.Registration.RegistrationDate"),
								Expression.GeProperty(
									Projections.SqlFunction("DATE_ADD", NHibernateUtil.DateTime,
										Projections.Property("u.Registration.RegistrationDate"),
										Projections.SqlProjection("INTERVAL 1 hour as ",
											new[] { string.Empty },
											new[] { NHibernateUtil.DateTime })),
									"ad.Registration.RegistrationDate")))
							.CreateCriteria("Users", "u", JoinType.InnerJoin)
							.SetProjection(Projections.ProjectionList()
								.Add(Projections.Conditional(
									Expression.Gt(
										Projections.Count(Projections.Distinct(Projections.Property("u.id"))), 1),
									Projections.SqlFunction("group_concat", NHibernateUtil.String, Projections.Distinct(Projections.Property("u.id"))),
									Projections.SqlFunction("group_concat", NHibernateUtil.String, Projections.Distinct(
										Projections.Property("u.id")),
										Projections.Constant(" - ("),
										Projections.Property("u.Name"),
										Projections.Constant(")")))))),
						"UserNames")))
					.Add(Expression.Ge("Registration.RegistrationDate", Period.Begin.Date))
					.Add(Expression.Le("Registration.RegistrationDate", Period.End.Date));

				return adressCriteria;
			}

			return null;
		}

		public IList<RegistrationInformation> Find()
		{
			var result = AcceptPaginator(GetCriteria());

			foreach (var registrationInformation in result) {
				registrationInformation.ObjectType = FinderType;
			}

			return result;
		}
	}
}