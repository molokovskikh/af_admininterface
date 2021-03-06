﻿using System;
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

		public int CurWeekUpdates { get; set; }
		public int LastWeekUpdates { get; set; }

		[Display(Name = "Обновления (Старый/Новый)", Order = 5)]
		public string Updates
		{
			get { return string.Format("{0}/{1}", LastWeekUpdates, CurWeekUpdates); }
		}

		public int CurWeekSum { get; set; }
		public int LastWeekSum { get; set; }

		[Display(Name = "Заказы (Старый/Новый)", Order = 7)]
		public string Orders
		{
			get { return string.Format("{0}/{1}", LastWeekSum.ToString("C"), CurWeekSum.ToString("C")); }
		}

		[Display(Name = "Падение обновлений %", Order = 6)]
		public decimal ProblemUpdates { get; set; }

		[Display(Name = "Падение заказов %", Order = 8)]
		public decimal ProblemOrders { get; set; }

		public int CurWeekAddresses { get; set; }
		public int LastWeekAddresses { get; set; }

		[Display(Name = "Адреса (Старый/Новый)", Order = 9)]
		public string Addresses
		{
			get
			{
				if (ForSubQuery)
					return "-";
				return string.Format("{0}/{1}", LastWeekAddresses, CurWeekAddresses);
			}
		}

		public AutoOrderStatus AutoOrderIs { get; set; }

		[Display(Name = "Автозаказ", Order = 10)]
		public string AutoOrder
		{
			get
			{
				if (ForSubQuery)
					return "-";
				return AutoOrderIs.GetDescription();
			}
		}

		public bool ForSubQuery;
		public AnalysisReportType ReportType;
		public bool ForExport;

		public uint GetClientId()
		{
			uint result = 0;
			uint.TryParse(_id ?? "", out result);
			return result;
		}
	}

	public enum AnalysisReportType
	{
		[Description("Clients")] Client,
		[Description("Users")] User,
		[Description("Addresses")] Address
	}

	public enum AutoOrderStatus
	{
		[Description("Не настроен")] NotTuned,
		[Description("Не используют")] NotUsed,
		[Description("Используют")] Used
	}

	public class AnalysisOfWorkDrugstoresFilter : PaginableSortable, IFiltrable<BaseItemForTable>
	{
		public ulong[] Regions { get; set; }
		public DatePeriod FistPeriod { get; set; }
		public DatePeriod LastPeriod { get; set; }
		public uint ObjectId { get; set; }
		public AnalysisReportType Type { get; set; }
		public bool ForSubQuery { get; set; }

		public int? AutoOrder { get; set; }

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

DROP TEMPORARY TABLE IF EXISTS orders.AutoOrderCntFirst;
CREATE TEMPORARY TABLE orders.AutoOrderCntFirst (INDEX idx(ClientId) USING HASH) engine MEMORY
SELECT u.ClientId, COUNT(u.id) as AutoOrderCnt
	FROM logs.analitfupdates au,
	customers.users u
	WHERE au.requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au.userid = u.id
	AND au.COMMIT = 1
	AND au.updatetype IN (10)
	and u.Enabled = 1
	and u.WorkRegionMask & :regionCode > 0
group by u.ClientId;

DROP TEMPORARY TABLE IF EXISTS orders.AutoOrderCntNetFirst;
CREATE TEMPORARY TABLE orders.AutoOrderCntNetFirst (INDEX idx(ClientId) USING HASH) engine MEMORY
SELECT u.ClientId, COUNT(u.id) as AutoOrderCntNet
	FROM logs.RequestLogs au,
	customers.users u
WHERE au.CreatedOn BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au.UserId = u.id
	AND au.IsCompleted = 1
	AND (au.UpdateType = 'BatchController' or au.UpdateType = 'SmartOrder')
	and u.Enabled = 1
	and u.WorkRegionMask & :regionCode > 0
group by u.ClientId;

drop temporary table if exists orders.CurWeekUpdates;
create temporary table orders.CurWeekUpdates (INDEX idx(ClientId) USING HASH) engine MEMORY
select u.ClientId, COUNT(au.UpdateId) + COUNT(reql.RequestToken)  as CurWeekUpdates
from customers.users u
left join
(SELECT * FROM logs.analitfupdates as an WHERE
an.requesttime between :FistPeriodStart AND :FistPeriodEnd
and an.Commit = 1
and an.updatetype in (1, 2)
) as au on au.userid = u.id
left join
(SELECT * FROM logs.RequestLogs as re WHERE
re.CreatedOn between :FistPeriodStart AND :FistPeriodEnd
and re.IsCompleted = 1
and re.IsFaulted = 0
and re.UpdateType = 'MainController'
) as reql ON reql.UserId =  u.Id
where u.Enabled = 1
and u.WorkRegionMask & :regionCode > 0
group by u.clientid
HAVING ClientId IS NOT NULL AND CurWeekUpdates > 0;

drop temporary table if exists orders.LastWeekUpdates;
create temporary table orders.LastWeekUpdates (INDEX idx(ClientId) USING HASH) engine MEMORY
select u.ClientId, COUNT(au.UpdateId) + COUNT(reql.RequestToken)  as LastWeekUpdates
from customers.users u
left join
(SELECT * FROM logs.analitfupdates as an WHERE
an.requesttime between :LastPeriodStart AND :LastPeriodEnd
and an.Commit = 1
and an.updatetype in (1, 2)
) as au on au.userid = u.id
left join
(SELECT * FROM logs.RequestLogs as re WHERE
re.CreatedOn between :LastPeriodStart AND :LastPeriodEnd
and re.IsCompleted = 1
and re.IsFaulted = 0
and re.UpdateType = 'MainController'
) as reql ON reql.UserId =  u.Id
where u.Enabled = 1
and u.WorkRegionMask & :regionCode > 0
group by u.clientid
HAVING ClientId IS NOT NULL AND LastWeekUpdates > 0;

insert into Customers.Updates
SELECT cd.id,
cd.name,
reg.Region as RegionName,
count(distinct u.Id) as UserCount,
count(distinct a.Id) as AddressCount,
cwu.CurWeekUpdates,
lwu.LastWeekUpdates,
osf.ClientSum as CurWeekSum,
osl.ClientSum as LastWeekSum,
osf.AddressCnt as CurWeekAddresses,
osl.AddressCnt as LastWeekAddresses,
IFNULL(sr.AssortimentPriceCode,4662) <> 4662 as AutoOrderIs,
aof.AutoOrderCnt,
aofNet.AutoOrderCntNet

FROM customers.Clients Cd
	join usersettings.RetClientsSet Rcs on rcs.clientcode = cd.id
	join farm.Regions reg on reg.RegionCode = Cd.regioncode
	left join orders.OrdersSumFirst osf on osf.ClientId = cd.Id
	left join orders.OrdersSumLast osl on osl.ClientId = cd.Id
	left join orders.AutoOrderCntFirst aof on aof.ClientId = cd.Id
	left join orders.AutoOrderCntNetFirst aofNet on aofNet.ClientId = cd.Id
	left join orders.CurWeekUpdates cwu on cwu.ClientId = cd.Id
	left join orders.LastWeekUpdates lwu on lwu.ClientId = cd.Id
	left join ordersendrules.smart_order_rules sr on sr.Id = rcs.SmartOrderRuleId
	left join Customers.Users u on u.ClientId = cd.id and u.Enabled = 1
	left join Customers.Addresses a on a.ClientId = cd.id and a.Enabled = 1
WHERE
	cd.regioncode & :regionCode > 0
	{0}
	And cd.Status = 1
	AND rcs.serviceclient = 0
	AND rcs.invisibleonfirm = 0
group by cd.id
HAVING UserCount > 0
;";

		public string UserTypeSqlPart = @"insert into Customers.Updates (Id, Name, CurWeekUpdates, LastWeekUpdates, CurWeekSum, LastWeekSum)
SELECT u.id,
u.name,
(SELECT COUNT(au1.Updateid)
	FROM logs.analitfupdates au1
	WHERE au1.requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au1.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) as CurWeekUpdates,
(SELECT COUNT(au2.Updateid)
	FROM logs.analitfupdates au2
	WHERE requesttime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND au2.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) LastWeekUpdates,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh1,
	orders.orderslist ol1
	WHERE writetime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND ol1.orderid = oh1.rowid
	AND oh1.UserId = u.id
	AND oh1.RegionCode & :regionCode > 0
) CurWeekSum,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.UserId = u.id
	AND oh2.RegionCode & :regionCode > 0
) LastWeekSum
FROM customers.Users u
WHERE
u.WorkRegionMask & :regionCode > 0
{0}
;";

		public string AddressTypeSqlPart = @"insert into Customers.Updates (Id, Name, CurWeekUpdates, LastWeekUpdates)
SELECT a.Id,
a.Address as Name,
(SELECT COUNT(au1.Updateid)
	FROM logs.analitfupdates au1
	WHERE au1.requesttime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND au1.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) as CurWeekUpdates,
(SELECT COUNT(au2.Updateid)
	FROM logs.analitfupdates au2
	WHERE requesttime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND au2.userid = u.id
	AND COMMIT = 1
	AND updatetype IN (1, 2)
) LastWeekUpdates
FROM customers.Addresses a
join customers.UserAddresses ua on ua.AddressId = a.Id
join customers.Users u on u.Id = ua.UserId and (u.WorkRegionMask & :regionCode) > 0
WHERE
u.WorkRegionMask & :regionCode > 0
{0}
group by a.id;

DROP TEMPORARY TABLE IF EXISTS Customers.AddressesUpdates;

CREATE TEMPORARY TABLE Customers.AddressesUpdates (
Id INT unsigned,
CurWeekSum INT,
LastWeekSum INT) engine=MEMORY;

insert into Customers.AddressesUpdates
SELECT a.id,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh1,
	orders.orderslist ol1
	WHERE writetime BETWEEN :FistPeriodStart AND :FistPeriodEnd
	AND ol1.orderid = oh1.rowid
	AND oh1.AddressId = a.id
	AND oh1.RegionCode & :regionCode > 0
) CurWeekSum,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.AddressId = a.id
	AND oh2.RegionCode & :regionCode > 0
) LastWeekSum
FROM customers.Addresses a
WHERE
1=1
{0};

update Customers.Updates u, Customers.AddressesUpdates au
set
u.CurWeekSum = au.CurWeekSum,
u.LastWeekSum = au.LastWeekSum
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
				{ "ProblemUpdates", "ProblemUpdates" },
				{ "ProblemOrders", "ProblemOrders" },
				{ "Updates", "CurWeekUpdates" },
				{ "Orders", "CurWeekSum" },
				{ "RegionName", "RegionName" },
				{ "AutoOrder", "AutoOrderIs" },
				{ "Addresses", "CurWeekAddresses" },
			};
			Regions = new ulong[]{ 1 };
		}

		public AnalysisOfWorkDrugstoresFilter(int pageSize) : this()
		{
			PageSize = pageSize;
		}

		public string GetRegionNames()
		{
			var result = "";
			if (Regions != null && Regions.Any())
				result = Session.Query<Region>().Where(x => Regions.Contains(x.Id)).Select(x => x.Name).OrderBy(x => x).ToList().Implode();
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
CurWeekUpdates INT,
LastWeekUpdates INT,
CurWeekSum INT,
LastWeekSum INT,
CurWeekAddresses INT,
LastWeekAddresses INT,
AutoOrderIs int not null default 0,
AutoOrderCnt int,
AutoOrderCntNet int
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

		public IList<AnalysisOfWorkFiled> GetResult(bool forExport)
		{
			PrepareAggregatesData();

			Session.CreateSQLQuery(@"update Customers.updates set AutoOrderIs = 2
where AutoOrderIs = 1 and (AutoOrderCnt > 0 or AutoOrderCntNet > 0);").ExecuteUpdate();

			var where = "";
			if (Type == AnalysisReportType.Client && AutoOrder.HasValue)
				where = $"where up.AutoOrderIs = {AutoOrder.Value}";

			RowsCount =
				Convert.ToInt32(Session.CreateSQLQuery($"select count(*) from Customers.updates up {where};").UniqueResult());

			var limitPart = string.Empty;
			if (!forExport)
				limitPart = $"limit {CurrentPage*PageSize}, {PageSize}";


			var result = Session.CreateSQLQuery(string.Format(@"
SELECT
	up.Id, up.Name, up.RegionName, up.UserCount, up.AddressCount,
	up.CurWeekUpdates, up.LastWeekUpdates,
	IF (LastWeekUpdates > CurWeekUpdates, ROUND((LastWeekUpdates-CurWeekUpdates)*100/LastWeekUpdates), 0) ProblemUpdates,
	up.CurWeekSum, up.LastWeekSum,
	IF (LastWeekSum > CurWeekSum, ROUND((LastWeekSum-CurWeekSum)*100/LastWeekSum), 0) ProblemOrders,
	up.CurWeekAddresses, up.LastWeekAddresses, up.AutoOrderIs
FROM Customers.updates up
{0}
group by up.Id
ORDER BY {1} {2} {3}", where, GetSortProperty(), SortDirection, limitPart))
				.ToList<AnalysisOfWorkFiled>();

			return result;
		}

		public IList<BaseItemForTable> Find(bool forExport)
		{
			var result = GetResult(forExport);

			foreach (var analysisOfWorkFiled in result)
				analysisOfWorkFiled.ForSubQuery = ForSubQuery;

			return result.Cast<BaseItemForTable>().ToList();
		}
	}
}