using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	public enum UpdateType
	{
		[Description("Накопительное")] Accumulative = 1,
		[Description("Кумулятивное")] Cumulative = 2,
		[Description("Отправка заказа")] OldOrderSending = 4,
		[Description("Ошибка доступа")] AccessError = 5,
		[Description("Ошибка сервера")] ServerError = 6,
		[Description("Документы")] Documents = 8,
		[Description("Загрузка документов на сервер")] LoadingDocuments = 9,
		[Description("АвтоЗаказ")] AutoOrder = 10,
		[Description("Отправка заказов")] NewOrderSending = 11,
		[Description("Отправка измененных настроек прайс-листов")] PriceSettingSending = 12,
		[Description("Загрузка отправленных заказов")] OrdersDownload = 13,
		[Description("Подтверждение сообщения для пользователя")] ConfirmUserMessage = 14,
		[Description("Асинхронное накопительное")] AccumulativeAsync = 16,
		[Description("Асинхронное кумулятивное")] CumulativeAsync = 17,
		[Description("Частичное кумулятивное")] LimitedCumulative = 18,
		[Description("Частичное асинхронное кумулятивное")] LimitedCumulativeAsync = 19,
	}

	[ActiveRecord(Table = "AnalitFUpdates", Schema = "logs")]
	public class UpdateLogEntity
	{
		public UpdateLogEntity()
		{}

		public UpdateLogEntity(User user)
		{
			RequestTime = DateTime.Now;
			UpdateType = UpdateType.Accumulative;
			User = user;
			UserName = user.Login;
		}

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

		public static bool IsDataTransferUpdateType(UpdateType updateType)
		{
			return updateType == UpdateType.Accumulative || updateType == UpdateType.Cumulative || updateType == UpdateType.LimitedCumulative 
				|| updateType == UpdateType.AccumulativeAsync || updateType == UpdateType.CumulativeAsync || updateType == UpdateType.LimitedCumulativeAsync || updateType == UpdateType.AutoOrder
				|| IsDocumentLoading(updateType);
		}

		public static bool IsDocumentLoading(UpdateType updateType)
		{
			return updateType == UpdateType.LoadingDocuments;
		}

		public IList<DocumentReceiveLog> GetLoadedDocumentLogs()
		{
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof(DocumentReceiveLog))
					.Add(Expression.Eq("SendUpdateLogEntity", this))
						.AddOrder(Order.Desc("LogTime"))
						.List<DocumentReceiveLog>());
		}
	}
}