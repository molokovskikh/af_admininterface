using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Security;
using Common.Tools.Calendar;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;
using Common.Tools;

namespace AdminInterface.Queries
{
	public class AnalysisOfWorkFiled : BaseItemForTable
	{
		private string _id;

		[Display(Name = "Код", Order = 0)]
		public string Id
		{
			get
			{
				if (ForExport)
					return _id;
				return AppHelper.LinkToNamed(_id, ReportType.GetDescription(), parameters: new { @params = new { Id = _id } });
			}
			set { _id = value; }
		}


		private string _name;

		[Display(Name = "Наименование", Order = 1)]
		public string Name
		{
			get
			{
				if (ForExport)
					return _name;
				return AppHelper.LinkToNamed(_name, ReportType.GetDescription(), parameters: new { @params = new { Id = _id } });
			}
			set { _name = value; }
		}

		private string _userCount;

		[Display(Name = "Количество пользователей", Order = 2)]
		public string UserCount
		{
			get
			{
				if (ForExport)
					return _userCount;
				if (!ForSubQuery)
					return string.Format("<a href=\"javascript:\" id=\"{1}\" onclick=\"GetAnalysInfo({1}, this, 'User')\">{0}</a>", _userCount, _id);
				else {
					return "-";
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
				if (ForSubQuery)
					return "-";
				return _addressCount;
			}
			set { _addressCount = value; }
		}

		private string _regionName;

		[Display(Name = "Регион", Order = 4)]
		public string RegionName {
			get
			{
				if (ForSubQuery)
					return "-";
				return _regionName;
			}
			set { _regionName = value; }
		}

		public int CurWeekObn { get; set; }
		public int LastWeekObn { get; set; }

		[Display(Name = "Обновления (Старый/Новый)", Order = 5)]
		public string Obn
		{
			get { return string.Format("{0}/{1}", LastWeekObn, CurWeekObn); }
		}

		public int CurWeekZak { get; set; }
		public int LastWeekZak { get; set; }

		[Display(Name = "Заказы (Старый/Новый)", Order = 7)]
		public string Zak
		{
			get { return string.Format("{0}/{1}", LastWeekZak.ToString("C"), CurWeekZak.ToString("C")); }
		}

		[Display(Name = "Падение обновлений %", Order = 6)]
		public decimal ProblemObn { get; set; }

		[Display(Name = "Падение заказов %", Order = 8)]
		public decimal ProblemZak { get; set; }

		public int CurWeekAddr { get; set; }
		public int LastWeekAddr { get; set; }

		[Display(Name = "Адреса (Старый/Новый)", Order = 9)]
		public string Addr
		{
			get
			{
				if (ForSubQuery)
					return "-";
				return string.Format("{0}/{1}", LastWeekAddr, CurWeekAddr);
			}
		}

		public int AvtozakazIs { get; set; }

		[Display(Name = "Автозаказ", Order = 10)]
		public string Avtozakaz
		{
			get
			{
				if (ForSubQuery)
					return "-";

				switch (AvtozakazIs) {
					case 2:
						return "Используют";
					case 1:
						return "Не используют";
					default:
						return "Не настроен";
				}
			}
		}

		public bool ForSubQuery;
		public AnalysisReportType ReportType;
		public bool ForExport;
	}

	public enum AnalysisReportType
	{
		[Description("Clients")] Client,
		[Description("Users")] User,
		[Description("Addresses")] Address
	}

	public class AnalysisOfWorkDrugstoresFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public ulong[] Regions { get; set; }
		public DatePeriod FistPeriod { get; set; }
		public DatePeriod LastPeriod { get; set; }
		public uint ObjectId { get; set; }
		public AnalysisReportType Type { get; set; }
		public bool ForSubQuery { get; set; }

		public int? Avtozakaz { get; set; }

		public IList<BaseItemForTable> Find()
		{
			return Find(false);
		}

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public string ClientTypeSqlPart = @"

DROP TEMPORARY TABLE IF EXISTS orders.OrdersSumFirst;
CREATE TEMPORARY TABLE orders.OrdersSumFirst (INDEX idx(ClientId) USING HASH) engine MEMORY
SELECT
	oh1.clientcode as ClientId,
	ROUND(sum(cost*quantity),2) as ClientSum,
	COUNT(distinct oh1.AddressId) as AddressCnt
FROM orders.ordershead oh1
join orders.orderslist ol1 on ol1.orderid = oh1.rowid
WHERE
	writetime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND oh1.RegionCode & :regionCode > 0
 group by oh1.clientcode;

DROP TEMPORARY TABLE IF EXISTS orders.OrdersSumLast;
CREATE TEMPORARY TABLE orders.OrdersSumLast (INDEX idx(ClientId) USING HASH) engine MEMORY
SELECT
	oh1.clientcode as ClientId,
	ROUND(sum(cost*quantity),2) as ClientSum,
	COUNT(distinct oh1.AddressId) as AddressCnt
FROM orders.ordershead oh1
join orders.orderslist ol1 on ol1.orderid = oh1.rowid
WHERE
	writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND oh1.RegionCode & :regionCode > 0
 group by oh1.clientcode;


insert into Customers.Updates
SELECT cd.id,
cd.name,
reg.Region as RegionName,
count(u.Id) as UserCount,
count(a.Id) as AddressCount,
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
osf.ClientSum as CurWeekZak,
osl.ClientSum as LastWeekZak,
osf.AddressCnt as CurWeekAddr,
osl.AddressCnt as LastWeekAddr,
IFNULL(sr.AssortimentPriceCode,4662) <> 4662 as AvtozakazIs,
(SELECT COUNT(o.id)
	FROM logs.analitfupdates au1,
	customers.users O
	WHERE requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au1.userid =o.id
	AND o.clientid=cd.id
	AND COMMIT = 1
	AND updatetype IN (10)
) as AvtozakazCnt


FROM customers.Clients Cd
	join usersettings.RetClientsSet Rcs on rcs.clientcode = cd.id
	join farm.Regions reg on reg.RegionCode = Cd.regioncode
	left join orders.OrdersSumFirst osf on osf.ClientId = cd.Id
	left join orders.OrdersSumLast osl on osl.ClientId = cd.Id
	left join ordersendrules.smart_order_rules sr on sr.Id = rcs.SmartOrderRuleId
	left join Customers.Users u on u.ClientId = cd.id
	left join Customers.Addresses a on a.ClientId = cd.id
WHERE
	cd.regioncode & :regionCode > 0
	{0}
	And cd.Status = 1
	AND rcs.serviceclient = 0
	AND rcs.invisibleonfirm = 0
group by cd.id;";

		public string UserTypeSqlPart = @"insert into Customers.Updates (Id, Name, CurWeekObn, LastWeekObn, CurWeekZak, LastWeekZak)
SELECT u.id,
u.name,
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
	AND oh1.RegionCode & :regionCode > 0
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.UserId = u.id
	AND oh2.RegionCode & :regionCode > 0
) LastWeekZak
FROM customers.Users u
WHERE
u.WorkRegionMask & :regionCode > 0
{0}
;";

		public string AddressTypeSqlPart = @"insert into Customers.Updates (Id, Name, CurWeekObn, LastWeekObn)
SELECT a.Id,
a.Address as Name,
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
	AND oh1.RegionCode & :regionCode > 0
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.AddressId = a.id
	AND oh2.RegionCode & :regionCode > 0
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
				{ "Obn", "CurWeekObn" },
				{ "Zak", "CurWeekZak" },
				{ "RegionName", "RegionName" },
				{ "Avtozakaz", "AvtozakazIs" },
				{ "Addr", "CurWeekAddr" },
			};
			Regions = new ulong[]{ 1 };
		}

		public AnalysisOfWorkDrugstoresFilter(int pageSize) : this()
		{
			PageSize = pageSize;
		}

		public string GetRegionNames(ISession session)
		{
			var result = "";
			if (Regions != null && Regions.Any())
				result = session.Query<Region>().Where(x => Regions.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList().Implode();
			return result;
		}

		public void PrepareAggregatesData()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Regions != null && Regions.Any())
			{
				ulong mask = 0;
				foreach (var region in Regions)
					mask |= region;
				regionMask &= mask;
			}

			string createTemporaryTablePart =
				@"DROP TEMPORARY TABLE IF EXISTS Customers.Updates;

CREATE TEMPORARY TABLE Customers.Updates (
Id INT unsigned,
Name varchar(50) ,
RegionName varchar(50),
UserCount INT,
AddressCount INT,
CurWeekObn INT,
LastWeekObn INT,
CurWeekZak INT,
LastWeekZak INT,
CurWeekAddr INT,
LastWeekAddr INT,
AvtozakazIs int not null default 0,
AvtozakazCnt int
) engine=MEMORY;";

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

		public IList<BaseItemForTable> Find(bool forExport)
		{
			PrepareAggregatesData();

			RowsCount = Convert.ToInt32(Session.CreateSQLQuery("select count(*) from Customers.updates;").UniqueResult());

			var limitPart = string.Empty;

			if (!forExport)
				limitPart = string.Format("limit {0}, {1}", CurrentPage * PageSize, PageSize);

			var where = "";
			if (Type == AnalysisReportType.Client && Avtozakaz.HasValue) {
				switch (Avtozakaz.Value) {
					case 1: // Не используют
						where = "where up.AvtozakazIs = 1 and IFNULL(up.AvtozakazCnt,0) = 0 ";
						break;
					case 2: // Используют
						where = "where up.AvtozakazIs = 1 and up.AvtozakazCnt > 0 ";
						break;
					default: // Не настроен
						where = "where up.AvtozakazIs = 0 ";
						break;
				}
			}

			var result = Session.CreateSQLQuery(string.Format(@"
SELECT
	up.Id, up.Name, up.RegionName, up.UserCount, up.AddressCount,
	up.CurWeekObn, up.LastWeekObn,
	IF (LastWeekObn > CurWeekObn, ROUND((LastWeekObn-CurWeekObn)*100/LastWeekObn), 0) ProblemObn,
	up.CurWeekZak, up.LastWeekZak,
	IF (LastWeekZak > CurWeekZak, ROUND((LastWeekZak-CurWeekZak)*100/LastWeekZak), 0) ProblemZak,
	up.CurWeekAddr, up.LastWeekAddr,
	IF (up.AvtozakazIs = 1 and up.AvtozakazCnt > 0, 2, up.AvtozakazIs) AvtozakazIs
FROM Customers.updates up
{0}
group by up.Id
ORDER BY {1} {2} {3}", where, GetSortProperty(), SortDirection, limitPart))
				.ToList<AnalysisOfWorkFiled>();

			foreach (var analysisOfWorkFiled in result) {
				analysisOfWorkFiled.ForSubQuery = ForSubQuery;
			}

			return result.Cast<BaseItemForTable>().ToList();
		}
	}
}