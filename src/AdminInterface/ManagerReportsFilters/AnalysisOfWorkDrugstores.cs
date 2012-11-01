using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AdminInterface.Security;
using Common.Tools.Calendar;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.ManagerReportsFilters
{
	public class AnalysisOfWorkFiled : BaseItemForTable
	{
		private string _id;

		[Display(Name = "Код", Order = 0)]
		public string Id
		{
			get { return Link(_id, ReportType.GetDescription(), new System.Tuple<string, object>("Id", _id)); }
			set { _id = value; }
		}


		private string _name;

		[Display(Name = "Наименование", Order = 1)]
		public string Name
		{
			get { return Link(_name, ReportType.GetDescription(), new System.Tuple<string, object>("Id", _id)); }
			set { _name = value; }
		}

		private string _userCount;

		[Display(Name = "Количество пользователей", Order = 2)]
		public string UserCount
		{
			get
			{
				if (!ForSubQuery)
					return string.Format("<a href=\"javascript:\" onclick=\"GetAnalysInfo({1}, this, 'User')\">{0}</a>", _userCount, _id);
				else {
					return string.Empty;
				}
			}
			set { _userCount = value; }
		}

		private string _addressCount;

		[Display(Name = "Количество адресов доставки", Order = 3)]
		public string AddressCount
		{
			get
			{
				if (!ForSubQuery) return _addressCount;
				else return string.Empty;
			}
			set { _addressCount = value; }
		}

		[Display(Name = "Регион", Order = 4)]
		public string RegionName { get; set; }

		public int CurWeekObn { get; set; }
		public int LastWeekObn { get; set; }

		[Display(Name = "Обновления (Новый/Старый)", Order = 5)]
		public string Obn
		{
			get { return string.Format("{0}/{1}", CurWeekObn, LastWeekObn); }
		}

		public int CurWeekZak { get; set; }
		public int LastWeekZak { get; set; }

		[Display(Name = "Заказы (Новый/Старый)", Order = 7)]
		public string Zak
		{
			get { return string.Format("{0}/{1}", CurWeekZak, LastWeekZak); }
		}

		[Display(Name = "Падение обновлений", Order = 6)]
		public int ProblemObn { get; set; }
		[Display(Name = "Падение заказов", Order = 8)]
		public int ProblemZak { get; set; }

		public bool ForSubQuery;
		public AnalysisReportType ReportType;
	}

	public enum AnalysisReportType
	{
		[Description("Clients")]
		Client,
		[Description("Users")]
		User,
		[Description("Addresses")]
		Address
	}

	public class AnalysisOfWorkDrugstoresFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public Region Region { get; set; }
		public DatePeriod FistPeriod { get; set; }
		public DatePeriod LastPeriod { get; set; }
		public uint ObjectId { get; set; }
		public AnalysisReportType Type { get; set; }
		public bool ForSubQuery { get; set; }

		public int PagesSize
		{
			get { return PageSize; }
			set { PageSize = value; }
		}

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public string ClientTypeSqlPart = @"insert into Customers.Updates
SELECT cd.id,
cd.name,
reg.Region as RegionName,
(SELECT COUNT(o.id)
	FROM logs.analitfupdates au1,
	customers.users O
	WHERE requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au1.userid =o.id
	AND o.clientid=cd.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) as CurWeekObn,
(SELECT COUNT(au2.Updateid)
	FROM logs.analitfupdates au2,
	customers.users O
	WHERE requesttime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND au2.userid = o.id
	AND o.clientid = cd.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) LastWeekObn,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh1,
	orders.orderslist ol1
	WHERE writetime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND ol1.orderid = oh1.rowid
	AND oh1.clientcode = cd.id
	AND oh1.RegionCode = :regionCode
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.clientcode = cd.id
	AND oh2.RegionCode = :regionCode
) LastWeekZak
FROM customers.Clients Cd
left join usersettings.RetClientsSet Rcs on rcs.clientcode = cd.id
left join farm.Regions reg on reg.RegionCode = Cd.regioncode
WHERE
cd.regioncode & :regionCode > 0
{0}
AND rcs.serviceclient = 0
AND rcs.invisibleonfirm = 0;";

		public string UserTypeSqlPart = @"insert into Customers.Updates
SELECT u.id,
u.name,
reg.Region as RegionName,
(SELECT COUNT(au1.Updateid)
	FROM logs.analitfupdates au1
	WHERE au1.requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au1.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) as CurWeekObn,
(SELECT COUNT(au2.Updateid)
	FROM logs.analitfupdates au2
	WHERE requesttime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND au2.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) LastWeekObn,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh1,
	orders.orderslist ol1
	WHERE writetime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND ol1.orderid = oh1.rowid
	AND oh1.UserId = u.id
	AND oh1.RegionCode = :regionCode
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.UserId = u.id
	AND oh2.RegionCode = :regionCode
) LastWeekZak
FROM customers.Users u
join farm.Regions reg on (reg.RegionCode & :regionCode) > 0
WHERE
u.WorkRegionMask & :regionCode > 0
{0}
;";

		public string AddressTypeSqlPart = @"insert into Customers.Updates (Id, Name, RegionName, CurWeekObn, LastWeekObn)
SELECT a.Id,
a.Address as Name,
reg.Region as RegionName,
(SELECT COUNT(au1.Updateid)
	FROM logs.analitfupdates au1
	WHERE au1.requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au1.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) as CurWeekObn,
(SELECT COUNT(au2.Updateid)
	FROM logs.analitfupdates au2
	WHERE requesttime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND au2.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) LastWeekObn
FROM customers.Addresses a
join customers.UserAddresses ua on ua.AddressId = a.Id
join customers.Users u on u.Id = ua.UserId and (u.WorkRegionMask & :regionCode) > 0
left join farm.Regions reg on (reg.RegionCode & u.WorkRegionMask) > 0
WHERE
u.WorkRegionMask & :regionCode > 0
{0}
group by a.id
;

DROP TEMPORARY TABLE IF EXISTS Customers.AddressesUpdates;

CREATE TEMPORARY TABLE Customers.AddressesUpdates (
Id INT unsigned,
CurWeekZak INT,
LastWeekZak INT) engine=MEMORY;

insert into Customers.AddressesUpdates
SELECT a.id,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh1,
	orders.orderslist ol1
	WHERE writetime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND ol1.orderid = oh1.rowid
	AND oh1.AddressId = a.id
	AND oh1.RegionCode = :regionCode
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.AddressId = a.id
	AND oh2.RegionCode = :regionCode
) LastWeekZak
FROM customers.Addresses a
WHERE
1=1
{0};

update Customers.Updates u, Customers.AddressesUpdates au
set
u.CurWeekZak = au.CurWeekZak,
u.LastWeekZak = au.LastWeekZak
where u.id = au.Id;
";

		public AnalysisOfWorkDrugstoresFilter()
		{
			var firstWeek = DateHelper.GetWeekInterval(DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek + 1)));
			var lastWeek = DateHelper.GetWeekInterval(DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek + 8)));
			FistPeriod = new DatePeriod(firstWeek.StartDate, firstWeek.EndDate);
			LastPeriod = new DatePeriod(lastWeek.StartDate, lastWeek.EndDate);
			SortBy = "Name";
			Type = AnalysisReportType.Client;
			SortKeyMap = new Dictionary<string, string> {
				{ "Id", "Id" },
				{ "Name", "Name" },
				{ "UserCount", "UserCount" },
				{ "AddressCount", "AddressCount" },
				{ "ProblemObn", "ProblemObn" },
				{ "ProblemZak", "ProblemZak" },
			};
		}

		public void SetDefaultRegion()
		{
			if (Region == null)
				Region = Session.Query<Region>().First(r => r.Name == "Воронеж");
		}

		public void PrepareAggregatesData()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;

			if (Region != null)
				regionMask &= Region.Id;

			string createTemporaryTablePart =
@"DROP TEMPORARY TABLE IF EXISTS Customers.Updates;

CREATE TEMPORARY TABLE Customers.Updates (
Id INT unsigned,
Name varchar(50) ,
RegionName varchar(50) ,
CurWeekObn INT,
LastWeekObn INT,
CurWeekZak INT,
LastWeekZak INT) engine=MEMORY;";

			var currentQuery = string.Empty;

			if (ObjectId > 0) {
				switch (Type) {
					case AnalysisReportType.Client:
						currentQuery = " and Cd.Id = :ObjectId";
						break;
					case AnalysisReportType.User:
						currentQuery = " and u.id = :ObjectId";
						break;
					case AnalysisReportType.Address:
						currentQuery = " and a.Id = :ObjectId";
						break;
				}
			}

			var queryText = string.Empty;

			switch (Type) {
				case AnalysisReportType.Client:
					queryText = ClientTypeSqlPart;
					break;
				case AnalysisReportType.User:
					queryText = UserTypeSqlPart;
					break;
				case AnalysisReportType.Address:
					queryText = AddressTypeSqlPart;
					break;
			}

			Session.CreateSQLQuery(createTemporaryTablePart).ExecuteUpdate();

			var query = Session.CreateSQLQuery(string.Format(queryText, currentQuery))
				.SetParameter("FistPeriodStart", FistPeriod.Begin)
				.SetParameter("FistPeriodEnd", FistPeriod.End)
				.SetParameter("LastPeriodStart", LastPeriod.Begin)
				.SetParameter("LastPeriodEnd", LastPeriod.End)
				.SetParameter("regionCode", regionMask);

			if (ObjectId > 0)
				query.SetParameter("ObjectId", ObjectId);

			query.ExecuteUpdate();
		}

		public IList<BaseItemForTable> Find()
		{
			PrepareAggregatesData();

			RowsCount = Convert.ToInt32(Session.CreateSQLQuery("select count(*) from Customers.updates;").UniqueResult());

			var result = Session.CreateSQLQuery(string.Format(@"
SELECT
	Id,
	Name,
	RegionName,
	(select count(u.Id) from Customers.Users u where u.ClientId = up.Id) as UserCount,
	(select count(a.Id) from Customers.Addresses a where a.ClientId = up.Id) as AddressCount,
	CurWeekObn,
	LastWeekObn,
	IF (CurWeekObn - LastWeekObn < 0 , ( IF (CurWeekObn <> 0, ROUND((LastWeekObn-CurWeekObn)*100/LastWeekObn), 100) ) ,0) ProblemObn,
	CurWeekZak,
	LastWeekZak,
	IF (CurWeekZak - LastWeekZak < 0 , ( IF (CurWeekZak <> 0, ROUND((LastWeekZak-CurWeekZak)*100/LastWeekZak), 100 ) ) ,0) ProblemZak
FROM Customers.updates up
ORDER BY {2} {3}
limit {0}, {1}", CurrentPage * PageSize, PageSize, SortBy, SortDirection))
				.ToList<AnalysisOfWorkFiled>();

			foreach (var analysisOfWorkFiled in result) {
				analysisOfWorkFiled.ForSubQuery = ForSubQuery;
			}

			return result.Cast<BaseItemForTable>().ToList();
		}
	}
}