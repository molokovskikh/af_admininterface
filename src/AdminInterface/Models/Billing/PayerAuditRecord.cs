using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class PayerAuditRecord : IAuditRecord
	{
		private string _message;

		public PayerAuditRecord()
		{
		}

		public PayerAuditRecord(Payer payer, string message, string comment = null)
		{
			Payer = payer;
			Administrator = SecurityContext.Administrator;
			UserName = Administrator.UserName;
			WriteTime = DateTime.Now;

			ObjectId = Payer.Id;
			ObjectType = LogObjectType.Payer;
			Name = Payer.Name;
			Message = message;
			Comment = comment;
		}

		public PayerAuditRecord(Payer payer, Account accounting, string comment = null)
		{
			Payer = payer;
			Administrator = SecurityContext.Administrator;
			UserName = Administrator.UserName;
			WriteTime = DateTime.Now;

			ObjectId = accounting.ObjectId;
			ObjectType = accounting.ObjectType;
			Name = accounting.Name;
			Comment = comment;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public string UserName { get; set; }

		[BelongsTo]
		public Administrator Administrator { get; set; }

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

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore)]
		public string Message
		{
			get { return _message; }
			set
			{
				_message = value;
				if (_message != null)
					_message = _message.Remove(0, 3);
			}
		}

		[Property]
		public bool IsHtml { get; set; }

		[Property]
		public bool ShowOnlyPayer { get; set; }

		[Property]
		public string Comment { get; set; }

		public static IList<PayerAuditRecord> Find(Payer payer)
		{
			return ActiveRecordLinqBase<PayerAuditRecord>.Queryable
				.Where(r => r.Payer == payer)
				.OrderByDescending(r => r.WriteTime)
				.ToList();
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
				Comment = Comment,
				ShowOnlyPayer = ShowOnlyPayer
			};
		}

		public static void DeleteAuditRecords(Account account)
		{
			ArHelper.WithSession(s => {
				s.CreateSQLQuery("delete from Billing.PayerAuditRecords where ObjectId = :id and ObjectType = :type")
					.SetParameter("id", account.ObjectId)
					.SetParameter("type", account.ObjectType)
					.ExecuteUpdate();
			});
		}
	}
}