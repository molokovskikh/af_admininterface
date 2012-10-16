using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Models.Audit;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord("ClientLogs", Schema = "logs")]
	public class ClientLogRecord : ActiveRecordBase<ClientLogRecord>, IAuditRecord
	{
		public ClientLogRecord()
		{
			OperatorName = SecurityContext.Administrator.UserName;
			LogTime = DateTime.Now;
		}

		public ClientLogRecord(Client client, string comment = null) : this()
		{
			Client = client;
			Comment = comment;
			ClientStatus = client.Status;
		}

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

		[Property]
		public virtual string Comment { get; set; }

		public static IList<ClientLogRecord> GetLogs(IEnumerable<uint> clients)
		{
			if (clients.Count() == 0)
				return Enumerable.Empty<ClientLogRecord>().ToList();

			return (List<ClientLogRecord>)Execute(
				(session, instance) => session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.ClientLogs {ClientLogRecord}
where status is not null
		and clientId in (:clientId)
order by logtime desc
limit 100")
					.AddEntity(typeof(ClientLogRecord))
					.SetParameterList("clientId", clients.ToList())
					.List<ClientLogRecord>(), null);
		}

		public static ClientLogRecord LastOff(Client client)
		{
			return (ClientLogRecord)Execute(
				(session, instance) => session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.ClientLogs {ClientLogRecord}
where status = 0
		and clientId = :ClientCode
order by logtime desc
limit 1")
					.AddEntity(typeof(ClientLogRecord))
					.SetParameter("ClientCode", client.Id)
					.UniqueResult(), null);
		}

		public string Message { get; set; }
		public bool IsHtml { get; set; }
	}
}