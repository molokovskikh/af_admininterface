using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public enum RegistrationFinderType
	{
		[Description("Пользователям")] Users,
		[Description("Адресам")] Addresses
	}

	public class RegistrationInformation : IUrlContributor
	{
		public uint Id { get; set; }

		private string _name;

		public string Name { get
		{
			if (_name == null)
				return Id.ToString();
			return _name;
		}
			set { _name = value; }
		}

		public DateTime RegistrationDate { get; set; }
		public bool AdressEnabled { get; set; }
		public ClientStatus ClientEnabled { get; set; }
		public bool UserEnabled { get; set; }
		public bool ServiceDisabled { get; set; }

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

		public IDictionary GetQueryString()
		{
			return new Dictionary<string, string> {
				{"controller", ObjectType.ToString()},
				{"action", "edit"},
				{"id", Id.ToString()},
			};
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
				{"Id", "Id"},
				{"Name", "Name"},
				{"RegistrationDate", "RegistrationDate"},
			};

			Period = new DatePeriod(DateTime.Now.AddDays(-1), DateTime.Now);
			FinderType = RegistrationFinderType.Users;
			Region = RegionHelper.GetAllRegions().Where(region => region.Name.ToLower().Equals("все")).First();
		}

		private IList<RegistrationInformation> AcceptPaganator(DetachedCriteria criteria)
		{
			DetachedCriteria countSubquery = NHibernate.CriteriaTransformer.TransformToRowCount(criteria);
			_lastRowsCount = ArHelper.WithSession(s => countSubquery.GetExecutableCriteria(s).UniqueResult<int>());

			if (CurrentPage > 0)
				criteria.SetFirstResult(CurrentPage*PageSize);

			criteria.SetMaxResults(PageSize);

			return ArHelper.WithSession(
				s => criteria.GetExecutableCriteria(s).ToList<RegistrationInformation>())
				.ToList();
		}

		public IList<RegistrationInformation> Find()
		{
			IList<RegistrationInformation> result = new List<RegistrationInformation>();

			if (FinderType == RegistrationFinderType.Users) {

				var userCriteria = DetachedCriteria.For<User>();

				userCriteria.CreateCriteria("RootService", "s", JoinType.InnerJoin)
					.Add(Expression.Sql("{alias}.HomeRegion & " + Region.Id + " > 0"));
				userCriteria.SetProjection(Projections.ProjectionList()
				                           	.Add(Projections.Property<User>(u => u.Id).As("Id"))
				                           	.Add(Projections.Property<User>(u => u.Name).As("Name"))
				                           	.Add(Projections.Property<User>(u => u.RegistrationDate).As("RegistrationDate"))
				                           	.Add(Projections.Property("Enabled").As("UserEnabled"))
				                           	.Add(Projections.Property("s.Disabled").As("ServiceDisabled")))
					.Add(Expression.Ge("RegistrationDate", Period.Begin.Date))
					.Add(Expression.Le("RegistrationDate", Period.End.Date))
					.CreateAlias("Payer", "p", JoinType.InnerJoin)
					.Add(Expression.Or(Expression.Gt("p.PayerID", 921u), Expression.Lt("p.PayerID", 921u)));

				result = AcceptPaganator(userCriteria);

				foreach (var registrationInformation in result) {
					registrationInformation.ObjectType = RegistrationFinderType.Users;
				}
			}

			if (FinderType == RegistrationFinderType.Addresses) {

				var adressCriteria = DetachedCriteria.For<Address>();

				adressCriteria.CreateCriteria("Client", "c", JoinType.InnerJoin)
					.Add(Expression.Sql("{alias}.RegionCode & " + Region.Id + " > 0"));

				adressCriteria.SetProjection(Projections.ProjectionList()
				                             	.Add(Projections.Property<Address>(u => u.Id).As("Id"))
				                             	.Add(Projections.Property<Address>(u => u.Value).As("Name"))
				                             	.Add(Projections.Property<Address>(u => u.RegistrationDate).As("RegistrationDate"))
				                             	.Add(Projections.Property("Enabled").As("AdressEnabled"))
				                             	.Add(Projections.Property("c.Status").As("ClientEnabled")))
					.Add(Expression.Ge("RegistrationDate", Period.Begin.Date))
					.Add(Expression.Le("RegistrationDate", Period.End.Date));

				result = AcceptPaganator(adressCriteria);

				foreach (var registrationInformation in result) {
					registrationInformation.ObjectType = RegistrationFinderType.Addresses;
				}
			}

			return result;
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
			PropertyBag["filter"] = userFilter;
			PropertyBag["Users"] = userFilter.Find();
		}
	}
}