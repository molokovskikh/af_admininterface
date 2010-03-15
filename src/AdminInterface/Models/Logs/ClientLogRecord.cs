using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.ClientLogs")]
	public class ClientLogRecord : ActiveRecordBase<ClientLogRecord>
	{
		[PrimaryKey("ID")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[Property("Status")]
		public virtual ClientStatus? ClientStatus { get; set; }

		public static IList<ClientLogRecord> GetClientLogRecords(Client client)
		{
			return (List<ClientLogRecord>) Execute(
			                               	(session, instance) =>
			                               	session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.ClientLogs {ClientLogRecord}
where status is not null
		and clientId = :ClientCode
order by logtime desc
limit 5")
			                               		.AddEntity(typeof (ClientLogRecord))
			                               		.SetParameter("ClientCode", client.Id)
			                               		.List<ClientLogRecord>(), null);
		}

		public static ClientLogRecord LastOff(uint clientCode)
		{
			return (ClientLogRecord)Execute(
											(session, instance) =>
											session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.ClientLogs {ClientLogRecord}
where status = 0
		and clientId = :ClientCode
order by logtime desc
limit 1")
												.AddEntity(typeof(ClientLogRecord))
												.SetParameter("ClientCode", clientCode)
												.UniqueResult(), null);
		}
	}
}