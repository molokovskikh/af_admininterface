using System;
using System.Collections.Generic;
using System.ComponentModel;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Model
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

		[BelongsTo(Column = "FirmCode")]
		public Client FromSupplier { get; set; }

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
			return ArHelper.WithSession<DocumentLogEntity>(
				session => session.CreateCriteria(typeof (DocumentLogEntity))
				           	.Add(Expression.Between("LogTime", beginDate, endDate))
				           	.Add(Expression.Eq(fieldName, client))
				           	.AddOrder(Order.Desc("LogTime"))
				           	.List<DocumentLogEntity>());
			
		}
	}
}