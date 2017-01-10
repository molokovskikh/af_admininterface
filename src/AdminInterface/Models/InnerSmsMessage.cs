using System;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "InnerSmsMessages", Schema = "telephony")]
	public class InnerSmsMessage
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo]
		public virtual Administrator UserFrom { get; set; }

		[BelongsTo]
		public virtual Administrator UserTo { get; set; }

		[Property]
		public virtual string Message { get; set; }

		[Property]
		public virtual string TargetAddress { get; set; }

		[Property]
		public virtual DateTime Date { get; set; }

		[Property]
		public virtual int? SmsId { get; set; }

		public virtual string SmsIdFormat {
			get { return SmsId.HasValue && SmsId.Value != 0 ? SmsId.ToString() : "не отправлено"; }
		}
	}
}