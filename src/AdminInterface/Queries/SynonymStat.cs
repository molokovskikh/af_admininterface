using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Security;
using Common.Tools;
using Common.Web.Ui.Helpers;
using ExcelLibrary.BinaryFileFormat;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Linq;

namespace AdminInterface.Queries
{
	public class SynonymStatUnit
	{
		public int ProductSynonymCreationCount { get; set; }
		public int ProductSynonymDeletionCount { get; set; }
		public int ProducerSynonymCreationCount { get; set; }
		public int ProducerSynonymDeletionCount { get; set; }

		public int TotalCreation
		{
			get { return ProductSynonymCreationCount + ProducerSynonymCreationCount; }
		}

		public int TotalDeletion
		{
			get { return ProductSynonymDeletionCount + ProducerSynonymDeletionCount; }
		}

		public int DescriptionOperationCount { get; set; }

		public string OperatorName { get; set; }
	}

	public class SynonymStat
	{
		public SynonymStat()
		{
			Period = new DatePeriod(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
		}

		public DatePeriod Period { get; set; }

		public List<SynonymStatUnit> Find(ISession session)
		{
			var stats = new List<SynonymStatUnit>();
			var begin = Period.Begin;
			var end = Period.End.AddDays(1);
			var results = session.CreateSQLQuery(@"
select l.OperatorName,
	sum(if(l.operation = 0, 1, 0)) CreateCount,
	sum(if(l.operation = 2, 1, 0)) DeleteCount
from Logs.SynonymLogs l
where l.LogTime >= :begin
	and l.LogTime < :end
group by l.OperatorName;")
				.SetParameter("begin", begin)
				.SetParameter("end", end)
				.List<object[]>();

			foreach (var result in results) {
				var stat = Allocate(stats, result);
				stat.ProductSynonymCreationCount = Convert.ToInt32(result[1]);
				stat.ProductSynonymDeletionCount = Convert.ToInt32(result[2]);
			}

			results = session.CreateSQLQuery(@"
select l.OperatorName,
	sum(if(l.operation = 0 or l.operation = 1, 1, 0)) CreateCount,
	sum(if(l.operation = 2, 1, 0)) DeleteCount
from logs.synonymFirmCrLogs l
where l.LogTime >= :begin
	and l.LogTime < :end
	and l.OperatorName <> 'ProcessingSvc'
	and l.OperatorName <> 'event_scheduler'
group by l.OperatorName
;")
				.SetParameter("begin", begin)
				.SetParameter("end", end)
				.List<object[]>();
			foreach (var result in results) {
				var stat = Allocate(stats, result);
				stat.ProducerSynonymCreationCount = Convert.ToInt32(result[1]);
				stat.ProducerSynonymDeletionCount = Convert.ToInt32(result[2]);
			}

			results = session.CreateSQLQuery(@"
select l.OperatorName, count(*) OperationCount
from logs.descriptionlogs l
where l.LogTime >= :begin
	and l.LogTime < :end
group by l.OperatorName
;")
				.SetParameter("begin", begin)
				.SetParameter("end", end)
				.List<object[]>();
			foreach (var result in results) {
				var stat = Allocate(stats, result);
				stat.DescriptionOperationCount = Convert.ToInt32(result[1]);
			}

			var names = stats.Select(s => s.OperatorName).ToArray();
			var administrators = session.Query<Administrator>().Where(a => names.Contains(a.UserName)).ToList();
			foreach (var stat in stats) {
				var adm = administrators.FirstOrDefault(a => a.UserName.Match(stat.OperatorName));
				if (adm != null) {
					stat.OperatorName = adm.ManagerName;
				}
			}

			return stats;
		}

		private static SynonymStatUnit Allocate(List<SynonymStatUnit> stats, object[] result)
		{
			var stat = stats.FirstOrDefault(r => r.OperatorName.Match(Convert.ToString(result[0])));
			if (stat == null) {
				stat = new SynonymStatUnit {
					OperatorName = Convert.ToString(result[0])
				};
				stats.Add(stat);
			}
			return stat;
		}
	}
}