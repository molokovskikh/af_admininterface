using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Orders", Table = "OrdersHead")]
	public class ClientOrder : ActiveRecordLinqBase<ClientOrder>
	{
		public ClientOrder()
		{}

		public ClientOrder(User user, Price price)
		{
			Client = user.Client;
			Region = Client.HomeRegion;
			Address = user.AvaliableAddresses[0];
			User = user;
			Price = price;
			Submited = true;
		}

		[PrimaryKey("RowId")]
		public virtual uint Id { get; set; }

		[BelongsTo("ClientCode")]
		public virtual Client Client { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region Region { get; set; }

		[BelongsTo("PriceCode")]
		public virtual Price Price { get; set; }

		[Property]
		public virtual bool Submited { get; set; }
	}
}