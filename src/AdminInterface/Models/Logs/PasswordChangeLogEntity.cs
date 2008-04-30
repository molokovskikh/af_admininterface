using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Expression;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.passwordchange")]
	public class PasswordChangeLogEntity : ActiveRecordBase<PasswordChangeLogEntity>
	{
		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public string ClientHost { get; set; }

		[Property]
		public DateTime LogTime { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public string TargetUserName { get; set; }


		public static IList<PasswordChangeLogEntity> GetByLogin(string login, DateTime beginDate, DateTime endDate)
		{
			return ActiveRecordMediator<PasswordChangeLogEntity>
				.FindAll(new[] {Order.Asc("LogTime")},
				         Expression.Eq("TargetUserName", login)
				         && Expression.Between("LogTime", beginDate, endDate));
		}
	}
}
