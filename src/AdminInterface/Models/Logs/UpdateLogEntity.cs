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
		public string AppVersion { get; set; }

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
			var u = ArHelper.WithSession(
				session => session.CreateCriteria(typeof (UpdateLogEntity))
							.Add(Expression.InG("User", client.GetUsers().ToList()))
				           	.Add(Expression.Between("RequestTime", beginDate, endDate))
				           	.AddOrder(Order.Desc("RequestTime"))
				           	.List<UpdateLogEntity>());
			Console.Write(u.Count);
			return u;
		}
	}
}