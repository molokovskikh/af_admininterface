using System;
using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Models.Audit;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	public enum RejectedMessageType
	{
		[Description("Неизвестный")] Unknown,
		[Description("Накладная")] Waybills,
		[Description("Отказ")] Reject,
		[Description("Прайс-лист")] Price,
		[Description("Мини-почта")] MiniMail
	}

	[ActiveRecord("EmailRejectLogs", Schema = "logs", SchemaAction = "none")]
	public class RejectedEmail : ActiveRecordLinqBase<RejectedEmail>
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

		[Property(Column = "`To`")]
		public virtual string To { get; set; }

		[Property(Column = "`MessageType`")]
		public virtual RejectedMessageType MessageType { get;  set; }

		[Property]
		public virtual string Subject { get; set; }

		public static RejectedEmail[] Find(string pattern, DateTime fromDate, DateTime toDate)
		{
			return FindAll(Order.Asc("LogTime"),
				(Expression.Ge("LogTime", fromDate)
					&& Expression.Le("LogTime", toDate.Add(new TimeSpan(23, 59, 59)))
					&& (Expression.Like("From", pattern, MatchMode.Anywhere)
						|| Expression.Like("Subject", pattern, MatchMode.Anywhere))));
		}
	}
}