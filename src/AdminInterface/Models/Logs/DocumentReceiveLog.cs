using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Models.Logs
{
	public enum DocumentType
	{
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Reject = 2,
		[Description("Документы от АК \"Инфорум\"")] InforoomDoc = 3
	}

	[ActiveRecord("DocumentSendLogs", Schema = "Logs")]
	public class DocumentSendLog : ActiveRecordBase<DocumentSendLog>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User ForUser { get; set; }

		[BelongsTo("DocumentId")]
		public virtual DocumentReceiveLog Received { get; set; }

		[BelongsTo("UpdateId")]
		public virtual UpdateLogEntity SendedInUpdate { get; set; }

		[Property]
		public virtual bool Committed { get; set; }

		public static DetachedCriteria GetCriteriaForView(DateTime begin, DateTime end)
		{
			return DetachedCriteria.For<DocumentSendLog>()
				.CreateAlias("Received", "r", JoinType.InnerJoin)
				.CreateAlias("SendedInUpdate", "su", JoinType.LeftOuterJoin)
				.CreateAlias("ForUser", "u", JoinType.InnerJoin)
				.CreateAlias("r.FromSupplier", "fs", JoinType.InnerJoin)
				.CreateAlias("r.ForClient", "fc", JoinType.InnerJoin)
				.CreateAlias("r.Address", "a", JoinType.InnerJoin)
				.CreateAlias("r.Document", "d", JoinType.LeftOuterJoin)
				.CreateAlias("r.SendUpdateLogEntity", "ru", JoinType.LeftOuterJoin)
				.Add(Expression.Between("r.LogTime", begin, end))
				.AddOrder(Order.Desc("r.LogTime"));
		}

		public static IList<DocumentSendLog> GetEnitiesForUser(User user, DateTime begin, DateTime end)
		{
			return ArHelper.WithSession(
				s => GetCriteriaForView(begin, end)
					.Add(Expression.Eq("ForUser", user))
					.GetExecutableCriteria(s)
					.List<DocumentSendLog>());
		}

		public static IList<DocumentSendLog> GetEnitiesForClient(Client client, DateTime begin, DateTime end)
		{
			return ArHelper.WithSession(
				s => GetCriteriaForView(begin, end)
					.Add(Expression.Eq("r.ForClient", client))
					.GetExecutableCriteria(s)
					.List<DocumentSendLog>());
		}
	}

	[ActiveRecord("Document_logs", Schema = "Logs")]
	public class DocumentReceiveLog : ActiveRecordBase<DocumentReceiveLog>
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier FromSupplier { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client ForClient { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[OneToOne(PropertyRef = "Log")]
		public virtual Document Document { get; set; }

		[BelongsTo("SendUpdateId")]
		public virtual UpdateLogEntity SendUpdateLogEntity { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string FileName { get; set; }

		[Property]
		public virtual string Addition { get; set; }

		[Property]
		public virtual ulong DocumentSize { get; set; }

		[Property]
		public virtual DocumentType DocumentType { get; set; }
	}
}