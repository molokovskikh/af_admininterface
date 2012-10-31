using System;
using System.Collections;
using System.Collections.Generic;
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
	public class AnalysisOfWorkFiled
	{
		public uint Id { get; set; }
		public string Name { get; set; }
		public string RegionName { get; set; }
		public int CurWeekObn { get; set; }
		public int LastWeekObn { get; set; }
		public int CurWeekZak { get; set; }
		public int LastWeekZak { get; set; }

		public int ProblemObn { get; set; }
		public int ProblemZak { get; set; }
	}

	public class AnalysisOfWorkDrugstoresFilter : PaginableSortable, IFiltrable<AnalysisOfWorkFiled>
	{
		public Region Region { get; set; }
		public DatePeriod FistPeriod { get; set; }
		public DatePeriod LastPeriod { get; set; }
		public int PagesSize
		{
			get { return PageSize; }
			set { PageSize = value; }
		}

		public ISession Session { get; set; }
		public bool LoadDefault { get; set; }

		public AnalysisOfWorkDrugstoresFilter()
		{
			PageSize = 30;
			var firstWeek = DateHelper.GetWeekInterval(DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek + 1)));
			var lastWeek = DateHelper.GetWeekInterval(DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek + 8)));
			FistPeriod = new DatePeriod(firstWeek.StartDate, firstWeek.EndDate);
			LastPeriod = new DatePeriod(lastWeek.StartDate, lastWeek.EndDate);
			SortBy = "Name";
		}

		public void SetDefaultRegion()
		{
			if (Region == null)
				Region = Session.Query<Region>().First(r => r.Name == "Воронеж");
		}

		public IList<AnalysisOfWorkFiled> Find()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			Session.CreateSQLQuery(@"DROP TEMPORARY TABLE IF EXISTS Customers.Updates;

CREATE TEMPORARY TABLE Customers.Updates (
Id INT unsigned,
Name varchar(50) ,
RegionName varchar(50) ,
CurWeekObn INT,
LastWeekObn INT,
CurWeekZak INT,
LastWeekZak INT) engine=MEMORY ;

insert into Customers.Updates
SELECT cd.id ,
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
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :LastPeriodStart AND :LastPeriodEnd
	AND ol2.orderid = oh2.rowid
	AND oh2.clientcode = cd.id
) LastWeekZak
FROM customers.Clients Cd
left join usersettings.RetClientsSet Rcs on rcs.clientcode = cd.id
left join farm.Regions reg on reg.RegionCode = Cd.regioncode
WHERE
cd.regioncode & :regionMask > 0
AND rcs.serviceclient = 0
AND rcs.invisibleonfirm = 0;")
				.SetParameter("FistPeriodStart", FistPeriod.Begin)
				.SetParameter("FistPeriodEnd", FistPeriod.End)
				.SetParameter("LastPeriodStart", LastPeriod.Begin)
				.SetParameter("LastPeriodEnd", LastPeriod.End)
				.SetParameter("regionMask", regionMask)
				.ExecuteUpdate();

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

			return result;
		}
	}
}