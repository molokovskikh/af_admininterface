using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "billing.MailSentHistory")]
	public class MailSentEntity : ActiveRecordBase<MailSentEntity>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public DateTime SentDate { get; set; }

		[Property]
		public bool IsDeleted { get; set; }

		[Property]
		public string Comment { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public uint PayerId { get; set; }

		public static MailSentEntity[] GetHistory(uint payerId)
		{
			return ActiveRecordMediator<MailSentEntity>.FindAll(new[] {Order.Desc("SentDate")},
			                                                    Expression.Eq("PayerId", payerId));
		}
	}
}
