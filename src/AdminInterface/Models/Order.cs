using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Orders", Table = "OrdersHead")]
	public class ClientOrder : ActiveRecordLinqBase<ClientOrder>
	{
		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client Client { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }
	}
}