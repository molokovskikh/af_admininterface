using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	public enum UpdateType
	{
		[Description("Накопительное")] Accumulative = 1,
		[Description("Кумулятивное")] Cumulative = 2,
		[Description("Ошибка доступа")] AccessError = 5,
		[Description("Ошибка сервера")] ServerError = 6,
		[Description("Документы")] Documents = 8
	}

	[ActiveRecord(Table = "logs.AnalitFUpdates")]
	public class UpdateLogEntity : ActiveRecordBase<UpdateLogEntity>
	{
		[PrimaryKey("UpdateId")]
		public uint Id { get; set; }

		[Property]
		public DateTime RequestTime { get; set; }

		[Property]
		public string AppVersion { get; set; }

		[Property]
		public string DbVersion { get; set; }

		[Property]
		public UpdateType UpdateType { get; set; }

		[Property]
		public uint ResultSize { get; set; }

		[Property]
		public string Addition { get; set; }

		[Property]
		public uint ClientCode { get; set; }

		[Property]
		public bool Commit { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public string Log { get; set; }

		[HasMany(typeof(UpdateDownloadLogEntity), Lazy = true, Inverse = true, OrderBy = "LogTime")]
		public IList<UpdateDownloadLogEntity> UpdateDownload { get; set; }

		public bool IsDataTransferUpdateType()
		{
			return UpdateType == UpdateType.Accumulative || UpdateType == UpdateType.Cumulative;
		}

		public static IList<UpdateLogEntity> GetEntitiesFormClient(uint clientCode, 
		                                                           DateTime beginDate, 
		                                                           DateTime endDate)
		{
			return ArHelper.WithSession<UpdateLogEntity>(
				session => session.CreateCriteria(typeof (UpdateLogEntity))
				           	.Add(Expression.Eq("ClientCode", clientCode))
				           	.Add(Expression.Between("RequestTime", beginDate, endDate))
				           	.AddOrder(Order.Desc("RequestTime"))
				           	.List<UpdateLogEntity>());
		}
	}
}