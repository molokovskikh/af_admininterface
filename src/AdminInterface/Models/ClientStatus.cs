using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "usersettings.ClientsData")]
	public class ClientWithStatus : ActiveRecordBase<ClientWithStatus>
	{
		[PrimaryKey]
		public uint FirmCode { get; set; }

		[Property("FirmStatus")]
		public virtual ClientStatus Status { get; set; }
	}
}
