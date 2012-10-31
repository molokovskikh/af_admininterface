using System;
using System.Collections;
using System.Collections.Generic;
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
		[Display(Name = "Код", Order = 0)]
		public uint Id { get; set; }
		[Display(Name = "Наименование", Order = 1)]
		public string Name { get; set; }
		[Display(Name = "Регион", Order = 2)]
		public string RegionName { get; set; }

		public int CurWeekObn { get; set; }
		public int LastWeekObn { get; set; }

		[Display(Name = "Обновления (Новый/Старый)", Order = 3)]
		public string Obn
		{
			get { return string.Format("{0}/{1}", CurWeekObn, LastWeekObn); }
		}

		public int CurWeekZak { get; set; }
		public int LastWeekZak { get; set; }

		[Display(Name = "Заказы (Новый/Старый)", Order = 5)]
		public string Zak
		{
			get { return string.Format("{0}/{1}", CurWeekZak, LastWeekZak); }
		}

		[Display(Name = "Падение обновлений", Order = 4)]
		public int ProblemObn { get; set; }
		[Display(Name = "Падение заказов", Order = 6)]
		public int ProblemZak { get; set; }
	}

	public enum AnalysisReportType
	{
		Client,
		User,
		Address
	}

	public class AnalysisOfWorkDrugstoresFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public Region Region { get; set; }
		public DatePeriod FistPeriod { get; set; }
		public DatePeriod LastPeriod { get; set; }
		public uint ObjectId { get; set; }
		public AnalysisReportType Type { get; set; }

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
				{ "Name", "Name" }
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
						currentQuery = " and a.Id = ObjectId";
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
	CurWeekObn,
	LastWeekObn,
	IF (CurWeekObn - LastWeekObn < 0 , ( IF (CurWeekObn <> 0, ROUND((LastWeekObn-CurWeekObn)*100/LastWeekObn), 100) ) ,0) ProblemObn,
	CurWeekZak,
	LastWeekZak,
	IF (CurWeekZak - LastWeekZak < 0 , ( IF (CurWeekZak <> 0, ROUND((LastWeekZak-CurWeekZak)*100/LastWeekZak), 100 ) ) ,0) ProblemZak
FROM Customers.updates
ORDER BY {2} {3}
limit {0}, {1}", CurrentPage * PageSize, PageSize, SortBy, SortDirection))
				.ToList<AnalysisOfWorkFiled>();

			return result.Cast<BaseItemForTable>().ToList();
		}
	}
}