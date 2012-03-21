using System;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Table = "MailSentHistory", Schema = "billing")]
	public class MailSentEntity : ActiveRecordValidationBase<MailSentEntity>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public DateTime SentDate { get; set; }

		[Property(NotNull = true), ValidateNonEmpty("Нужно ввести комментарий")]
		public string Comment { get; set; }

		[Property(NotNull = true)]
		public string UserName { get; set; }

		[Property]
		public uint PayerId { get; set; }

		public static MailSentEntity[] GetHistory(Payer payer)
		{
			return ActiveRecordLinqBase<MailSentEntity>.Queryable
				.Where(m => m.PayerId == payer.PayerID)
				.OrderByDescending(m => m.SentDate)
				.ToArray();
		}
	}
}
