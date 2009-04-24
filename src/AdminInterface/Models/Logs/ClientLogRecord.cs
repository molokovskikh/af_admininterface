using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("logs.clients_data_logs")]
	public class ClientLogRecord : ActiveRecordBase<ClientLogRecord>
	{
		[PrimaryKey("ID")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[Property("FirmStatus")]
		public virtual ClientStatus? ClientStatus { get; set; }

		public static IList<ClientLogRecord> GetClientLogRecords(uint clientCode)
		{
			return (List<ClientLogRecord>) Execute(
			                               	(session, instance) =>
			                               	session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.clients_data_logs {ClientLogRecord}
where firmstatus is not null
		and clientsdataId = :ClientCode
order by logtime desc
limit 5")
			                               		.AddEntity(typeof (ClientLogRecord))
			                               		.SetParameter("ClientCode", clientCode)
			                               		.List<ClientLogRecord>(), null);
		}

		public static ClientLogRecord LastOff(uint clientCode)
		{
			return (ClientLogRecord)Execute(
											(session, instance) =>
											session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.clients_data_logs {ClientLogRecord}
where firmstatus = 0
		and clientsdataId = :ClientCode
order by logtime desc
limit 1")
												.AddEntity(typeof(ClientLogRecord))
												.SetParameter("ClientCode", clientCode)
												.UniqueResult(), null);
		}
	}
}