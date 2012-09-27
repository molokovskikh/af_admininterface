using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Type;
using DocumentType = AdminInterface.Models.Logs.DocumentType;

namespace AdminInterface.Controllers
{
	public enum RegistrationFinderType
	{
		[Description("Пользователям")] Users,
		[Description("Адресам")] Addresses
	}

	public class RejectCounts
	{
		public uint ClientId { get; set; }
		public uint AddressId { get; set; }
		public int Count { get; set; }
		public string ClientName { get; set; }
		public string AddressName { get; set; }
		public ClientStatus ClientStatus { get; set; }
		public bool AddressEnabled { get; set; }
		public bool IsUpdate { get; set; }
		public string RegionName { get; set; }
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
				{ "ClientName", "ClientName" }
			};
			PageSize = 30;
			Period = new DatePeriod(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
		}

		private IList<RejectCounts> AcceptPaginator(DetachedCriteria criteria)
		{
			RowsCount = ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).ToList<RejectCounts>()).Count;

			if (CurrentPage > 0)
				criteria.SetFirstResult(CurrentPage * PageSize);

			criteria.SetMaxResults(PageSize);

			ApplySort(criteria);

			return ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).ToList<RejectCounts>()).ToList();
		}

		private DetachedCriteria GetCriteria()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var criteria = DetachedCriteria.For<RejectWaybillLog>();

			criteria.CreateCriteria("ForClient", "c", JoinType.LeftOuterJoin)
				.Add(Expression.Sql("{alias}.RegionCode & " + regionMask + " > 0"));
			criteria.CreateAlias("Address", "a", JoinType.LeftOuterJoin);

			criteria.SetProjection(Projections.ProjectionList()
				.Add(Projections.Count("Id").As("Count"))
				.Add(Projections.GroupProperty("c.Id").As("ClientId"))
				.Add(Projections.GroupProperty("a.Id").As("AddressId"))
				.Add(Projections.Property("c.Name").As("ClientName"))
				.Add(Projections.Property("a.Value").As("AddressName"))
				.Add(Projections.Property("c.Status").As("ClientStatus"))
				.Add(Projections.Property("a.Enabled").As("AddressEnabled"))
				.Add(Projections.Property("c.HomeRegion").As("RegionName")));
			criteria.Add(Expression.Ge("LogTime", Period.Begin.Date))
				.Add(Expression.Le("LogTime", Period.End.Date));
			return criteria;
		}

		public List<Address> Sort(List<Address> list)
		{
			var sortProperty = GetSortProperty();
			switch (sortProperty) {
				case "Id":
					if(SortDirection == "asc")
						return list.OrderBy(t => t.Id).ToList();
					return list.OrderByDescending(t => t.Id).ToList();
				case "c.Id":
					if(SortDirection == "asc")
						return list.OrderBy(t => t.Client.Id).ToList();
					return list.OrderByDescending(t => t.Client.Id).ToList();
				case "Name":
					if(SortDirection == "asc")
						return list.OrderBy(t => t.Value).ToList();
					return list.OrderByDescending(t => t.Value).ToList();
				case "c.Name":
					if(SortDirection == "asc")
						return list.OrderBy(t => t.Client.Name).ToList();
					return list.OrderByDescending(t => t.Client.Name).ToList();
			}
			return new List<Address>();
		}

		public IList<RejectCounts> Find()
		{
			var criteria = GetCriteria();
			var result = AcceptPaginator(criteria);
			ArHelper.WithSession(s => {
				foreach (var row in result) {
					var address = s.Query<Address>().FirstOrDefault(a => a.Id == row.AddressId);
					if(address != null)
						row.IsUpdate = address.AvaliableForUsers.Count(u => u.Logs.AFTime >= DateTime.Now.AddMonths(-1)) != 0;
				}
			});
			return result;
		}
	}

	public class RegistrationInformation : IUrlContributor
	{
		public uint Id { get; set; }

		private string _name;

		public string Name
		{
			get
			{
				if (_name == null)
					return Id.ToString();
				return _name;
			}
			set { _name = value; }
		}
		public bool IsUpdate { get; set; }
		public DateTime RegistrationDate { get; set; }
		public bool AdressEnabled { get; set; }
		public ClientStatus ClientEnabled { get; set; }
		public bool UserEnabled { get; set; }
		public bool ServiceDisabled { get; set; }

		public uint ClientId { get; set; }
		public string ClientName { get; set; }

		public int UserCount { get; set; }
		public string UserNames { get; set; }
		public string RegionName { get; set; }

		public ServiceType ClientType { get; set; }

		public RegistrationFinderType ObjectType;

		[Style]
		public bool DisabledByBilling
		{
			get
			{
				return (ObjectType == RegistrationFinderType.Users && (!UserEnabled || ServiceDisabled)) ||
					(ObjectType == RegistrationFinderType.Addresses && (!AdressEnabled || ClientEnabled == ClientStatus.Off));
			}
		}

		[Style]
		public bool SingleUser
		{
			get { return UserCount == 1; }
		}

		public IDictionary GetQueryString()
		{
			return new Dictionary<string, string> {
				{ "controller", ObjectType.ToString() },
				{ "action", "edit" },
				{ "id", Id.ToString() },
			};
		}

		public bool IsDrugstore
		{
			get { return ClientType == ServiceType.Drugstore; }
		}
	}

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

	[
		Helper(typeof(PaginatorHelper), "paginator"),
		Secure(PermissionType.ManagerReport),
	]
	public class ManagerReportsController : ARSmartDispatcherController
	{
		public void UsersAndAdresses()
		{
			var userFilter = new UserFinderFilter();
			BindObjectInstance(userFilter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = userFilter;
			PropertyBag["Users"] = userFilter.Find();
		}

		public void GetUsersAndAdresses([DataBind("filter")] UserFinderFilter userFilter)
		{
			this.RenderFile("Пользовалети_и_адреса.xls", ExportModel.GetUserOrAdressesInformation(userFilter));
		}

		public void ClientAddressesMonitor()
		{
			var filter = new ClientAddressFilter();
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["Clients"] = filter.Find();
			PropertyBag["filter"] = filter;
		}
	}
}