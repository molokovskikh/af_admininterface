using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Logs
{
	public interface IAuditRecord
	{
		string Message { get; set; }
	}

	public interface IAuditable
	{
		IAuditRecord GetAuditRecord();
	}

	[ActiveRecord(Schema = "Billing")]
	public class PayerAuditRecord : IAuditRecord
	{
		public PayerAuditRecord()
		{
		}

		public PayerAuditRecord(Payer payer, Account accounting)
		{
			Payer = payer;
			Administrator = SecurityContext.Administrator;
			UserName = Administrator.UserName;
			WriteTime = DateTime.Now;

			ObjectId = accounting.ObjectId;
			ObjectType = accounting.ObjectType;
			Name = accounting.Name;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public string UserName { get; set; }

		[BelongsTo]
		public Administrator Administrator { get; set;}

		[Property]
		public DateTime WriteTime { get; set; }

		[BelongsTo]
		public Payer Payer { get; set; }

		[Property]
		public uint ObjectId { get; set; }

		[Property]
		public LogObjectType ObjectType { get; set; }

		[Property]
		public string Name { get; set; }

		[Property]
		public string Message { get; set; }

		public static IList<PayerAuditRecord> Find(Payer payer)
		{
			return ActiveRecordLinqBase<PayerAuditRecord>.Queryable
				.Where(r => r.Payer == payer)
				.OrderByDescending(r => r.WriteTime).ToList();
		}

		public AuditLogRecord ToAuditRecord()
		{
			return new AuditLogRecord {
				ObjectId = ObjectId,
				LogTime = WriteTime,
				LogType = ObjectType,
				OperatorName = UserName,
				Message = Message,
				Name = Name,
			};
		}
	}
}