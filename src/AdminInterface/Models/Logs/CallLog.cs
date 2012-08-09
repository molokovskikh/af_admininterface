using System;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	public enum CallDirection
	{
		Input = 0,
		Output = 1
	}

	public enum IdentificationStatus
	{
		Know = 0,
		Unknow = 1
	}

	[ActiveRecord(Table = "CallLogs", Schema = "logs")]
	public class CallLog : ActiveRecordLinqBase<CallLog>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property(Column = "`From`")]
		public virtual string From { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property(Column = "Id2")]
		public virtual IdentificationStatus Id2 { get; set; }

		[Property]
		public virtual CallDirection Direction { get; set; }

		public static string[] LastCalls()
		{
			//сломан хибер
/*			return (from call in Queryable
				where call.Id2 == IdentificationStatus.Unknow && call.Direction == CallDirection.Input
				orderby call.LogTime descending
				group call by call.From into c
				select c.Key).Take(5).ToArray();
 */
			var criteria = DetachedCriteria.For<CallLog>()
				.Add(Restrictions.Where<CallLog>(c => c.Id2 == IdentificationStatus.Unknow && c.Direction == CallDirection.Input))
				.SetProjection(Projections.Group<CallLog>(l => l.From))
				.AddOrder(Order.Desc("LogTime"))
				.SetMaxResults(5);

			return ArHelper.WithSession(s => criteria.GetExecutableCriteria(s).List<string>().ToArray());

		}
	}
}