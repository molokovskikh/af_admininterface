using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("Client", Schema = "Future")]
	public class ClientWithStatus : ActiveRecordBase<ClientWithStatus>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual ClientStatus Status { get; set; }
	}
}
