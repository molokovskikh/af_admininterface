using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
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
	public enum ExcludesTypes
	{
		[Description("Показывать всех")]
		All,
		[Description("Скрытых в интерфейсе поставщика")]
		Hidden,
		[Description("Не имеющих права заказа")]
		NoOrderPermission,
	}

	public class UserFinderFilter : Sortable, IPaginable
	{
		public Region Region { get; set; }
		public RegistrationFinderType FinderType { get; set; }
		public DatePeriod Period { get; set; }
		public ExcludesTypes ExcludeType { get; set; }

		private int _lastRowsCount;

		public int RowsCount
		{
			get { return _lastRowsCount; }
		}

		public int PageSize
		{
			get { return 100; }
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

		private IList<RegistrationInformation> AcceptPaginator(DetachedCriteria criteria, ISession session)
		{
			var countSubquery = CriteriaTransformer.TransformToRowCount(criteria);
			_lastRowsCount = countSubquery.GetExecutableCriteria(session).UniqueResult<int>();

			if (CurrentPage > 0)
				criteria.SetFirstResult(CurrentPage * PageSize);

			criteria.SetMaxResults(PageSize);

			ApplySort(criteria);

			return criteria.GetExecutableCriteria(session).ToList<RegistrationInformation>().ToList();
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

			var addressCountProjection = Projections.SubQuery(DetachedCriteria.For<Client>()
				.CreateAlias("Addresses", "a", JoinType.InnerJoin)
				.Add(Expression.EqProperty("Id", "c.Id"))
				.SetProjection(Projections.ProjectionList()
					.Add(Projections.Count("a.Id"))));

			var noOrderCriteria = DetachedCriteria.For<User>()
				.CreateAlias("Client", "cSub", JoinType.LeftOuterJoin)
				.CreateAlias("cSub.Settings", "setSub", JoinType.LeftOuterJoin);

			NoOrderPermission(noOrderCriteria);

			noOrderCriteria.Add(Expression.EqProperty("Id", "rootUser.Id"))
				.Add(Expression.EqProperty("cSub.Id", "c.Id"))
				.Add(Expression.EqProperty("setSub.Id", "set.Id"));

			noOrderCriteria.SetProjection(Projections.ProjectionList()
				.Add(Projections.Count("Id")));

			var noOrderProjection = Projections.SubQuery(noOrderCriteria);


			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			if (FinderType == RegistrationFinderType.Users) {
				var userCriteria = DetachedCriteria.For<User>("rootUser");

				userCriteria.CreateCriteria("RootService", "s", JoinType.InnerJoin)
					.Add(Expression.Sql("{alias}.HomeRegion & " + regionMask + " > 0"))
					.SetProjection(Projections.ProjectionList().Add(Projections.SqlProjection("{alias}.HomeRegion as RegionName", new[] { "RegionName" }, new[] { NHibernateUtil.String })));

				userCriteria.SetProjection(Projections.ProjectionList()
					.Add(Projections.Property<User>(u => u.Id).As("Id"))
					.Add(Projections.Property<User>(u => u.Name).As("Name"))
					.Add(Projections.Property<User>(u => u.Registration.RegistrationDate).As("RegistrationDate"))
					.Add(Projections.Property("rootUser.Enabled").As("UserEnabled"))
					.Add(Projections.Property("s.Id").As("ClientId"))
					.Add(Projections.Property("s.Name").As("ClientName"))
					.Add(Projections.Property("s.Disabled").As("ServiceDisabled"))
					.Add(Projections.Property("s.Type").As("ClientType"))
					.Add(Projections.Property("s.HomeRegion").As("RegionName"))
					.Add(Projections.Property("set.InvisibleOnFirm").As("InvisibleOnFirm"))
					.Add(Projections.Alias(userCountProjection, "UserCount"))
					.Add(Projections.Alias(addressCountProjection, "AddressCount"))
					.Add(Projections.Alias(noOrderProjection, "NoOrder")))
					.Add(Expression.Ge("Registration.RegistrationDate", Period.Begin))
					.Add(Expression.Le("Registration.RegistrationDate", Period.End))
					.CreateAlias("Payer", "p", JoinType.InnerJoin)
					.CreateAlias("Client", "c", JoinType.LeftOuterJoin)
					.CreateAlias("c.Settings", "set", JoinType.LeftOuterJoin)
					.Add(Expression.Or(Expression.Gt("p.Id", 921u), Expression.Lt("p.Id", 921u)))
					.Add(Expression.Eq("s.Type", ServiceType.Drugstore));

				if (ExcludeType == ExcludesTypes.Hidden)
					userCriteria.Add(Expression.Eq("set.InvisibleOnFirm", DrugstoreType.Standart));

				if (ExcludeType == ExcludesTypes.NoOrderPermission) {
					NoOrderPermission(userCriteria);
				}

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
					.Add(Projections.Alias(addressCountProjection, "AddressCount"))
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
					.Add(Expression.Ge("Registration.RegistrationDate", Period.Begin))
					.Add(Expression.Le("Registration.RegistrationDate", Period.End));

				return adressCriteria;
			}

			return null;
		}

		private void NoOrderPermission(DetachedCriteria criteria)
		{
			criteria.Add(Expression.Gt("OrderRegionMask", (ulong)0))
				.Add(Expression.Eq("SubmitOrders", false))
				.Add(Expression.Eq("set.ServiceClient", false))
				.Add(Expression.Eq("set.InvisibleOnFirm", DrugstoreType.Standart))
				.Add(Expression.Gt(
					Projections.Alias(Projections.SubQuery(DetachedCriteria.For<User>("user")
						.CreateAlias("AssignedPermissions", "ap")
						.Add(Expression.EqProperty("user.Id", "rootUser.Id"))
						.Add(Expression.Eq("ap.Id", 1u))
						.SetProjection(Projections.ProjectionList()
							.Add(Projections.Count("ap.Id")))), "per"), 0));
		}

		public IList<RegistrationInformation> Find(ISession session)
		{
			var result = AcceptPaginator(GetCriteria(), session);

			foreach (var registrationInformation in result) {
				registrationInformation.ObjectType = FinderType;
			}

			return result;
		}
	}
}