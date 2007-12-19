using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate;

namespace AdminInterface.Model
{
	[ActiveRecord("logs.clients_data_logs")]
	public class ClientLogRecord : ActiveRecordBase<ClientLogRecord>
	{
		private DateTime _logTime;
		private string _operatorName;
		private ClientStatus? _clientStatus;
		private uint _id;

		[PrimaryKey("ID")]
		public virtual uint Id
		{
			get { return _id;}
			set { _id = value; }
		}

		[Property]
		public virtual DateTime LogTime
		{
			get { return _logTime; }
			set { _logTime = value; }
		}

		[Property]
		public virtual string OperatorName
		{
			get { return _operatorName; }
			set { _operatorName = value; }
		}

		[Property("FirmStatus")]
		public virtual ClientStatus? ClientStatus
		{
			get { return _clientStatus; }
			set { _clientStatus = value; }
		}

		public static IList<ClientLogRecord> GetClientLogRecords(uint clientCode)
		{
			return (List<ClientLogRecord>) Execute(
			                               	delegate(ISession session, object instance)
			                               		{
			                               			return session.CreateSQLQuery(@"
select {ClientLogRecord.*}
from logs.clients_data_logs {ClientLogRecord}
where firmstatus is not null
		and clientsdataId = :ClientCode
order by logtime desc
limit 5")
			                               				.AddEntity(typeof (ClientLogRecord))
														.SetParameter("ClientCode", clientCode)
			                               				.List<ClientLogRecord>();
			                               		}, null);
		}
	}
}