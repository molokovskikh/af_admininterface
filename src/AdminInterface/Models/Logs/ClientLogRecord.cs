using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Common.Tools;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("ClientLogs", Schema = "logs")]
	public class ClientLogRecord : ActiveRecordBase<ClientLogRecord>
	{
		[PrimaryKey("ID")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string OperatorName { get; set; }

		[BelongsTo("ClientId")]
		public virtual Client Client { get; set; }

		[Property("Status")]
		public virtual ClientStatus? ClientStatus { get; set; }

		public static IList<ClientLogRecord> GetLogs(IEnumerable<Client> clients)
		{
			if (clients.Count() == 0)
				return Enumerable.Empty<ClientLogRecord>().ToList();

			return (List<ClientLogRecord>) Execute(
				(session, instance) => session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.ClientLogs {ClientLogRecord}
where status is not null
		and clientId in (:clientId)
order by logtime desc
limit 100")
						.AddEntity(typeof (ClientLogRecord))
						.SetParameterList("clientId", clients.Select(c => c.Id).ToList())
						.List<ClientLogRecord>(), null);
		}

		public static ClientLogRecord LastOff(Client client)
		{
			return (ClientLogRecord) Execute(
				(session, instance) => session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.ClientLogs {ClientLogRecord}
where status = 0
		and clientId = :ClientCode
order by logtime desc
limit 1")
						.AddEntity(typeof (ClientLogRecord))
						.SetParameter("ClientCode", client.Id)
						.UniqueResult(), null);
		}
	}
}