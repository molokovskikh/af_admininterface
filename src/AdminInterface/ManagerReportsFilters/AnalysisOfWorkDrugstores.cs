using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Security;
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

	public class AnalysisOfWorkDrugstoresFilter : PaginableSortable
	{
		public Region Region { get; set; }
		public DatePeriod Period { get; set; }
		protected ISession Session;

		public AnalysisOfWorkDrugstoresFilter(ISession session)
		{
			PageSize = 30;
			Session = session;
			Period = new DatePeriod(DateTime.Now.AddDays(-14), DateTime.Now);
			Region = Session.Query<Region>().First(r => r.Name == "Воронеж");
			SortBy = "Name";
		}

		public IList<AnalysisOfWorkFiled> Find()
		{
			var regionMask = SecurityContext.Administrator.RegionMask;
			if (Region != null)
				regionMask &= Region.Id;

			var result = Session.CreateSQLQuery(@"
DROP TEMPORARY TABLE IF EXISTS Customers.Updates;

CREATE TEMPORARY TABLE Customers.Updates (
Id INT unsigned,
Name varchar(50) ,
RegionName varchar(50) ,
CurWeekObn INT unsigned,
LastWeekObn INT unsigned,
CurWeekZak INT unsigned,
LastWeekZak INT unsigned) engine=MEMORY ;

insert into Customers.Updates
SELECT cd.id ,
cd.name,
reg.Region as RegionName,
(SELECT COUNT(o.id)
	FROM logs.analitfupdates au1,
	customers.users O
	WHERE requesttime BETWEEN :DateStart AND :DateEnd
	AND au1.userid =o.id
	AND o.clientid=cd.id
	AND COMMIT =1
	#AND updatetype IN
) as CurWeekObn,
(SELECT COUNT(au2.Updateid)
	FROM logs.analitfupdates au2,
	customers.users O
	WHERE requesttime BETWEEN :DateStart AND :DateEnd
	AND au2.userid =o.id
	AND o.clientid=cd.id
	AND COMMIT =1
	#AND updatetype IN
) LastWeekObn,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh1,
	orders.orderslist ol1
	WHERE writetime BETWEEN :DateStart AND :DateEnd
	AND ol1.orderid =oh1.rowid
	AND oh1.clientcode=cd.id
) CurWeekZak,
(SELECT ROUND(sum(cost*quantity), 2)
	FROM orders.ordershead oh2,
	orders.orderslist ol2
	WHERE writetime BETWEEN :DateStart AND :DateEnd
	AND ol2.orderid =oh2.rowid
	AND oh2.clientcode=cd.id
) LastWeekZak
FROM customers.Clients Cd
left join usersettings.RetClientsSet Rcs on rcs.clientcode = cd.id
left join farm.Regions reg on reg.RegionCode = Cd.regioncode
WHERE firmtype = 1
AND cd.regioncode & :regionMask > 0
AND rcs.serviceclient = 0
AND rcs.invisibleonfirm = 0;


SELECT
	Name,
	CurWeekObn,
	LastWeekObn,
	IF (CurWeekObn  -LastWeekObn < 0 , ( IF (CurWeekObn <> 0, ROUND((LastWeekObn-CurWeekObn)*100/LastWeekObn), 100) ) ,0) ProblemObn,
	CurWeekZak,
	LastWeekZak,
	IF (CurWeekZak-LastWeekZak < 0 , ( IF (CurWeekZak <> 0, ROUND((LastWeekZak-CurWeekZak)*100/LastWeekZak), 100 ) ) ,0) ProblemZak
FROM Customers.updates #having problem = 1
ORDER BY problemZak DESC, Name")
			.SetParameter("DateStart", Period.Begin)
			.SetParameter("DateEnd", Period.End)
			.SetParameter("regionMask", regionMask)
			.ToList<AnalysisOfWorkFiled>();

			return result;
		}
	}
}