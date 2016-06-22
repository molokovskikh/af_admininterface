using System;
using System.ComponentModel;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Models.Audit;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

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
	public class RejectedEmail
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

		public static RejectedEmail[] Find(ISession session, string pattern, DateTime fromDate, DateTime toDate)
		{
			return session.Query<RejectedEmail>()
				.Where(x => x.LogTime >= fromDate && x.LogTime <= toDate.Add(new TimeSpan(23, 59, 59))
					&& (x.From.Contains(pattern) || x.Subject.Contains(pattern)))
				.OrderBy(x => x.LogTime).ToArray();
		}
	}
}