using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Web.Ui.Helpers;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using System.Linq;

namespace AdminInterface.Models.Logs
{
	public enum DocumentType
	{
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Jeject = 2,
		[Description("Документ АК \"Инфорум\"")] DocumentFromInforoom = 3,
	}

	[ActiveRecord(Table = "logs.document_logs")]
	public class DocumentLogEntity : ActiveRecordBase<DocumentLogEntity>
	{
		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public DateTime LogTime { get; set; }

		[Property]
		public DocumentType? DocumentType { get; set; }

		[BelongsTo(Column = "ClientCode")]
		public Client ForClient { get; set; }

		[BelongsTo(Column = "AddressId")]
		public Address Address { get; set; }

		[BelongsTo(Column = "FirmCode")]
		public Supplier FromSupplier { get; set; }

		[Property]
		public string FileName { get; set; }

		[Property]
		public uint? DocumentSize { get; set; }

		[BelongsTo(Column = "UpdateId")]
		public UpdateLogEntity UpdateLogEntity { get; set; }

		[Property]
		public string Addition { get; set; }

		public static IList<DocumentLogEntity> GetEnitiesForClient(Client client, 
			DateTime beginDate, 
			DateTime endDate)
		{
			var fieldName = client.Type == ClientType.Supplier ? "FromSupplier" : "ForClient";
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof (DocumentLogEntity))
					.Add(Expression.Between("LogTime", beginDate, endDate))
					.Add(Expression.Eq(fieldName, client))
					.AddOrder(Order.Desc("LogTime"))
					.List<DocumentLogEntity>());
			
		}

		public static IList<DocumentLogEntity> GetEnitiesForUser(User user,
			DateTime beginDate,
			DateTime endDate)
		{
			var updateEntities = UpdateLogEntity.GetEntitiesByUser(user.Id, beginDate, endDate).ToArray();
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof(DocumentLogEntity))
					.Add(Expression.Between("LogTime", beginDate, endDate))
					.Add(Expression.In("UpdateLogEntity", updateEntities))
					.AddOrder(Order.Desc("LogTime"))
					.List<DocumentLogEntity>());

		}
	}
}