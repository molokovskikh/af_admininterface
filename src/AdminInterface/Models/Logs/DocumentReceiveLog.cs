using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using AddUser;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Common.Web.Ui.Models;
using NHibernate;

namespace AdminInterface.Models.Logs
{
	public enum DocumentType
	{
		[Description("Накладная")] Waybill = 1,
		[Description("Отказ")] Reject = 2,
		[Description("Документы от АК \"Инфорум\"")] InforoomDoc = 3
	}

	public enum CheckType
	{
		Ready = 1,
		[Description("Адрес отключен")] AddressDisable = 2,
		[Description("Адрес недоступен поставщику")]AddressNotAssign = 3,
		[Description("Пользователь не обновлялся более месяца")]UserNotUpdate = 4
	}

	[ActiveRecord("DocumentSendLogs", Schema = "Logs", Lazy = true)]
	public class DocumentSendLog
	{
		public DocumentSendLog()
		{
		}

		public DocumentSendLog(User user, DocumentReceiveLog document)
		{
			ForUser = user;
			Received = document;
			Committed = true;
			FileDelivered = true;
			DocumentDelivered = true;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User ForUser { get; set; }

		[BelongsTo("DocumentId", Lazy = FetchWhen.OnInvoke)]
		public virtual DocumentReceiveLog Received { get; set; }

		[BelongsTo("UpdateId")]
		public virtual UpdateLogEntity SendedInUpdate { get; set; }

		[Property]
		public virtual bool Committed { get; set; }

		[Property]
		public virtual bool FileDelivered { get; set; }

		[Property]
		public virtual bool DocumentDelivered { get; set; }
	}

	[ActiveRecord("Document_logs", Schema = "Logs", Lazy = true)]
	public class DocumentReceiveLog
	{
		public DocumentReceiveLog()
		{
		}

		public DocumentReceiveLog(Supplier supplier)
		{
			FromSupplier = supplier;
			LogTime = DateTime.Now;
			DocumentType = DocumentType.Waybill;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier FromSupplier { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client ForClient { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[OneToOne(PropertyRef = "Log")]
		public virtual FullDocument Document { get; set; }

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

		[HasMany(Inverse = true, Lazy = true)]
		public virtual IList<DocumentSendLog> SendLogs { get; set; }

		[BelongsTo("FirmCode")]
		public virtual Supplier Supplier { get; set; }

		public virtual string GetRemoteFileName(AppConfig config)
		{
			if (String.IsNullOrEmpty(FileName))
				return null;
			if (FromSupplier == null)
				return null;
			if (Address == null)
				return null;

			var file = String.Format("{0}_{1}({2}){3}", Id, FromSupplier.Name, Path.GetFileNameWithoutExtension(FileName), Path.GetExtension(FileName));
			return Path.Combine(config.AptBox, Address.Id.ToString(), DocumentType.ToString() + "s", file);
		}

		public virtual String CheckReason
		{
			get
			{
				var result = new List<string>();
				if (!Address.Enabled || !Address.Client.Enabled)
					result.Add(CheckType.AddressDisable.GetDescription());

				if ((Address.Client.MaskRegion & Supplier.RegionMask) == 0)
					result.Add(CheckType.AddressNotAssign.GetDescription());
				var holder = ActiveRecordMediator.GetSessionFactoryHolder();
				var session = holder.CreateSession(typeof(ActiveRecordBase));
				try {
					var lastUpdate = session.CreateSQLQuery(@"select max(AFTime)
from logs.AuthorizationDates d
join Customers.UserAddresses ua on ua.UserId = d.UserId
where ua.AddressId = :addressId")
						.SetParameter("addressId", Address.Id)
						.UniqueResult<DateTime?>();
					if (lastUpdate == null || lastUpdate.Value < DateTime.Now.AddMonths(-1))
						result.Add(CheckType.UserNotUpdate.GetDescription());
				}
				finally {
					holder.ReleaseSession(session);
				}
				return String.Join(", ", result);
			}
		}
	}
}