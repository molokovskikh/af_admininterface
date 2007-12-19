using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate;
using NHibernate.Expression;

namespace AdminInterface.Model
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
		private uint _id;
		private DateTime _logTime;
		private string _appVersion;
		private string _dbVersion;
		private UpdateType _updateType;
		private uint _resultSize;
		private string _addition;
		private uint _clientCode;
		private bool _commit;
		private string _userName;
		private string _log;

		[PrimaryKey("UpdateId")]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property]
		public DateTime RequestTime
		{
			get { return _logTime; }
			set { _logTime = value; }
		}

		[Property]
		public string AppVersion
		{
			get { return _appVersion; }
			set { _appVersion = value; }
		}

		[Property]
		public string DbVersion
		{
			get { return _dbVersion; }
			set { _dbVersion = value; }
		}

		[Property]
		public UpdateType UpdateType
		{
			get { return _updateType; }
			set { _updateType = value; }
		}

		[Property]
		public uint ResultSize
		{
			get { return _resultSize; }
			set { _resultSize = value; }
		}

		[Property]
		public string Addition
		{
			get { return _addition; }
			set { _addition = value; }
		}

		[Property]
		public uint ClientCode
		{
			get { return _clientCode; }
			set { _clientCode = value; }
		}

		[Property]
		public bool Commit
		{
			get { return _commit; }
			set { _commit = value; }
		}

		[Property]
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		[Property]
		public string Log
		{
			get { return _log; }
			set { _log = value; }
		}

		public bool IsDataTransferUpdateType()
		{
			return _updateType == UpdateType.Accumulative || _updateType == UpdateType.Cumulative;
		}


		public static IList<UpdateLogEntity> GetEntitiesFormClient(uint clientCode, 
																		  DateTime beginDate, 
																		  DateTime endDate)
		{
			return ArHelper.WithSession<UpdateLogEntity>(
				delegate(ISession session)
					{
						return session.CreateCriteria(typeof (UpdateLogEntity))
									.Add(Expression.Eq("ClientCode", clientCode))
									.Add(Expression.Between("RequestTime", beginDate, endDate))
									.AddOrder(Order.Desc("RequestTime"))
									.List<UpdateLogEntity>();
					});
		}
	}
}