using System;
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
		[Description("Адресам")] Adresses
	}

	public class RegistrationInformation
	{
		public uint Id { get; set; }
		public string Name { get; set; }
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
				       (ObjectType == RegistrationFinderType.Adresses && (!AdressEnabled || ClientEnabled == ClientStatus.Off));
			}
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

				DetachedCriteria countSubquery = NHibernate.CriteriaTransformer.TransformToRowCount(userCriteria);
				_lastRowsCount = ArHelper.WithSession(s => countSubquery.GetExecutableCriteria(s).UniqueResult<int>());

				if (CurrentPage > 0)
					userCriteria.SetFirstResult(CurrentPage*PageSize);

				userCriteria.SetMaxResults(PageSize);

				result = ArHelper.WithSession(
					s => userCriteria.GetExecutableCriteria(s).ToList<RegistrationInformation>())
					.ToList();

				foreach (var registrationInformation in result) {
					registrationInformation.ObjectType = RegistrationFinderType.Users;
				}
			}

			if (FinderType == RegistrationFinderType.Adresses) {

				var udressCriteria = DetachedCriteria.For<Address>();

				udressCriteria.CreateCriteria("Client", "c", JoinType.InnerJoin)
					.Add(Expression.Sql("{alias}.RegionCode & " + Region.Id + " > 0"));

				udressCriteria.SetProjection(Projections.ProjectionList()
				                             	.Add(Projections.Property<Address>(u => u.Id).As("Id"))
				                             	.Add(Projections.Property<Address>(u => u.Value).As("Name"))
				                             	.Add(Projections.Property<Address>(u => u.RegistrationDate).As("RegistrationDate"))
				                             	.Add(Projections.Property("Enabled").As("AdressEnabled"))
				                             	.Add(Projections.Property("c.Status").As("ClientEnabled")))
					.Add(Expression.Ge("RegistrationDate", Period.Begin.Date))
					.Add(Expression.Le("RegistrationDate", Period.End.Date));

				DetachedCriteria countSubquery = NHibernate.CriteriaTransformer.TransformToRowCount(udressCriteria);
				_lastRowsCount = ArHelper.WithSession(s => countSubquery.GetExecutableCriteria(s).UniqueResult<int>());

				if (CurrentPage > 0)
					udressCriteria.SetFirstResult(CurrentPage*PageSize);

				udressCriteria.SetMaxResults(PageSize);

				result = ArHelper.WithSession(
					s => udressCriteria.GetExecutableCriteria(s).ToList<RegistrationInformation>())
					.ToList();

				foreach (var registrationInformation in result) {
					registrationInformation.ObjectType = RegistrationFinderType.Adresses;
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