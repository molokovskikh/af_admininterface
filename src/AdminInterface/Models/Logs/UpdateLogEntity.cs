using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
		public uint AppVersion { get; set; }

		[Property]
		public UpdateType UpdateType { get; set; }

		[Property]
		public uint ResultSize { get; set; }

		[Property]
		public string Addition { get; set; }

		[Property]
		public bool Commit { get; set; }

		[Property]
		public string UserName { get; set; }

		[BelongsTo("UserId")]
		public User User { get; set; }

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
			var client = Client.Find(clientCode);
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof (UpdateLogEntity))
					.Add(Expression.InG("User", client.GetUsers().ToList()))
					.Add(Expression.Between("RequestTime", beginDate, endDate))
					.AddOrder(Order.Desc("RequestTime"))
					.List<UpdateLogEntity>());
		}

		public static IList<UpdateLogEntity> GetEntitiesByUser(uint userId,
			DateTime beginDate,
			DateTime endDate)
		{
			var user = User.Find(userId);
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof(UpdateLogEntity))
					.Add(Expression.Eq("User", user))
					.Add(Expression.Between("RequestTime", beginDate, endDate))
					.AddOrder(Order.Desc("RequestTime"))
					.List<UpdateLogEntity>());
		}

		public static IList<UpdateLogEntity> GetEntitiesByUpdateType(UpdateType? updateType, ulong regionMask, DateTime beginDate, DateTime endDate)
		{
			return ArHelper.WithSession(session => session.CreateCriteria(typeof(UpdateLogEntity))
					.Add(Expression.InG("User", User.FindAll())))
					.Add(Expression.Between("RequestTime", beginDate, endDate))
					.Add(Expression.Eq("UpdateType", updateType))
					.AddOrder(Order.Desc("RequestTime"))
					.List<UpdateLogEntity>();					
		}
	}
}