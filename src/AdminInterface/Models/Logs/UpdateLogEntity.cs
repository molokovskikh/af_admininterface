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
		[Description("�������������")] Accumulative = 1,
		[Description("������������")] Cumulative = 2,
		[Description("������ �������")] AccessError = 5,
		[Description("������ �������")] ServerError = 6,
		[Description("���������")] Documents = 8
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

	public static class UpdateLogEntityExtension
	{
		public static IList<UpdateLogEntity> SortBy(this IList<UpdateLogEntity> logEntities, string columnName, bool descending)
		{
			if (String.IsNullOrEmpty(columnName))
				return logEntities;
			if (columnName.Equals("RequestTime", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.RequestTime).ToList();
				return logEntities.OrderBy(entity => entity.RequestTime).ToList();
			}
			if (columnName.Equals("ResultSize", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.ResultSize).ToList();
				return logEntities.OrderBy(entity => entity.ResultSize).ToList();
			}
			if (columnName.Equals("Addition", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.Addition).ToList();
				return logEntities.OrderBy(entity => entity.Addition).ToList();
			}
			if (columnName.Equals("UserName", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.UserName).ToList();
				return logEntities.OrderBy(entity => entity.UserName).ToList();
			}
			if (columnName.Equals("Login", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.User.Login).ToList();
				return logEntities.OrderBy(entity => entity.User.Login).ToList();
			}
			if (columnName.Equals("ClientName", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.User.Client.Name).ToList();
				return logEntities.OrderBy(entity => entity.User.Client.Name).ToList();
			}
			if (columnName.Equals("UpdateType", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.UpdateType).ToList();
				return logEntities.OrderBy(entity => entity.UpdateType).ToList();
			}
			if (columnName.Equals("AppVersion", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.AppVersion).ToList();
				return logEntities.OrderBy(entity => entity.AppVersion).ToList();
			}
			if (columnName.Equals("Region", StringComparison.OrdinalIgnoreCase))
			{
				if (descending)
					return logEntities.OrderByDescending(entity => entity.User.Client.HomeRegion.Name).ToList();
				return logEntities.OrderBy(entity => entity.User.Client.HomeRegion.Name).ToList();
			}
			return logEntities;
		}		
	}
}