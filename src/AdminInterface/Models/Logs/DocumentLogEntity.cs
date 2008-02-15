using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Expression;
using Castle.ActiveRecord;

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
		private uint _id;
		private DateTime _logTime;
		private DocumentType? _documentType;
		private Client _forClient;
		private Client _fromSupplier;
		private string _fileName;
		private uint? _documentFileSize;
		private UpdateLogEntity _updateLogEntity;
		private string _addition;

		[PrimaryKey("RowId")]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property]
		public DateTime LogTime
		{
			get { return _logTime; }
			set { _logTime = value; }
		}

		[Property]
		public DocumentType? DocumentType
		{
			get { return _documentType; }
			set { _documentType = value; }
		}

		[BelongsTo(Column = "ClientCode")]
		public Client ForClient
		{
			get { return _forClient; }
			set { _forClient = value; }
		}

		[BelongsTo(Column = "FirmCode")]
		public Client FromSupplier
		{
			get { return _fromSupplier; }
			set { _fromSupplier = value; }
		}

		[Property]
		public string FileName
		{
			get { return _fileName; }
			set { _fileName = value; }
		}

		[Property]
		public uint? DocumentSize
		{
			get { return _documentFileSize; }
			set { _documentFileSize = value; }
		}

		[BelongsTo(Column = "UpdateId")]
		public UpdateLogEntity UpdateLogEntity
		{
			get { return _updateLogEntity; }
			set { _updateLogEntity = value; }
		}

		[Property]
		public string Addition
		{
			get { return _addition; }
			set { _addition = value; }
		}

		public static IList<DocumentLogEntity> GetEnitiesForClient(Client client, 
																		  DateTime beginDate, 
																		  DateTime endDate)
		{
			string fieldName = client.Type == ClientType.Supplier ? "FromSupplier" : "ForClient";
			return ArHelper.WithSession<DocumentLogEntity>(
					delegate(ISession session)
						{
							return session.CreateCriteria(typeof (DocumentLogEntity))
											.Add(Expression.Between("LogTime", beginDate, endDate))
											.Add(Expression.Eq(fieldName, client))
											.AddOrder(Order.Desc("LogTime"))
											.List<DocumentLogEntity>();
						});
			
		}
	}
}