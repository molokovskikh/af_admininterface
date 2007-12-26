using System.Collections;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "usersettings.ClientsData")]
	public class ClientWithStatus : ActiveRecordBase<ClientWithStatus>
	{
		private uint _firmCode;
		private ClientStatus _status;

		[PrimaryKey]
		public uint FirmCode
		{
			get { return _firmCode; }
			set { _firmCode = value; }
		}

		[Property("FirmStatus")]
		public virtual ClientStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}
	}
}
