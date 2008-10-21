using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.EmailRejectLogs")]
	public class RejectedEmail : ActiveRecordBase<RejectedEmail>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual uint SmtpId { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		[Property(Column = "`From`")]
		public virtual string From { get; set; }

		[Property]
		public virtual string Subject { get; set; }

		public static RejectedEmail[] Find(string pattern, DateTime fromDate, DateTime toDate)
		{
			return FindAll(Order.Asc("LogTime"),
			               Expression.Between("LogTime", fromDate, toDate.Add(new TimeSpan(23, 59, 59)))
			               && (Expression.Like("From", pattern, MatchMode.Anywhere)
			                   || Expression.Like("Subject", pattern, MatchMode.Anywhere)));

		}
	}
}
