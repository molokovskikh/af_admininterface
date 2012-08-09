using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Common.Tools;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("UserLogs", Schema = "logs")]
	public class UserLogRecord : ActiveRecordBase<UserLogRecord>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[Property]
		public virtual string Login { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual bool? Enabled { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[Property]
		public virtual LogOperation Operation { get; set; }

		[Property]
		public virtual string Comment { get; set; }

		public static IList<UserLogRecord> GetLogs(IEnumerable<User> users)
		{
			if (users.Count() == 0)
				return Enumerable.Empty<UserLogRecord>().ToList();

			return (List<UserLogRecord>)Execute(
				(session, instance) =>
					session.CreateSQLQuery(@"
select {UserLogRecord.*}
from `logs`.`UserLogs` {UserLogRecord}
where {UserLogRecord}.Enabled is not null
		and {UserLogRecord}.UserId in ( :clientId )
order by logtime desc
limit 100")
				.AddEntity(typeof(UserLogRecord))
				.SetParameterList("clientId", users.Select(u => u.Id).ToList())
				.List<UserLogRecord>(), null);
		}

		public static UserLogRecord LastOff(uint userId)
		{
			return (UserLogRecord)Execute(
				(session, instance) =>
					session.CreateSQLQuery(@"
select {UserLogRecord.*}
from logs.UserLogs {UserLogRecord}
where Enabled = 0
		and UserId = :UserId
order by logtime desc
limit 1")
				.AddEntity(typeof(UserLogRecord))
				.SetParameter("UserId", userId)
				.UniqueResult(), null);
		}

	}
}
