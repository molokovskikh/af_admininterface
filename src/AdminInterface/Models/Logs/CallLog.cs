using System;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

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

	[ActiveRecord(Table = "logs.CallLogs")]
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
			return (from call in Queryable
			        where call.Id2 == IdentificationStatus.Unknow && call.Direction == CallDirection.Input
					orderby call.LogTime
			        group call by call.From into c
			        select c.Key).Take(5).ToArray();
		}
	}
}